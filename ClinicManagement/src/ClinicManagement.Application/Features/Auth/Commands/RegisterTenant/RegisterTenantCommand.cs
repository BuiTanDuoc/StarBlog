using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Services;
using ClinicManagement.Application.Features.Auth.DTOs;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;
using ClinicManagement.Shared.Constants;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Application.Features.Auth.Commands.RegisterTenant;

/// <summary>Public self-service signup: a new clinic + its first user (ClinicOwner) + a Free trial subscription.</summary>
public record RegisterTenantCommand : IRequest<Result<AuthResponseDto>>
{
    public string ClinicName { get; init; } = default!;
    public string Slug { get; init; } = default!;
    public string OwnerFullName { get; init; } = default!;
    public string OwnerEmail { get; init; } = default!;
    public string Password { get; init; } = default!;
    public string? IpAddress { get; init; }
}

public class RegisterTenantCommandValidator : AbstractValidator<RegisterTenantCommand>
{
    public RegisterTenantCommandValidator()
    {
        RuleFor(x => x.ClinicName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Slug).NotEmpty().MaximumLength(100)
            .Matches("^[a-z0-9-]+$").WithMessage("Slug may only contain lowercase letters, numbers, and hyphens.");
        RuleFor(x => x.OwnerFullName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.OwnerEmail).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8)
            .Matches("[A-Z]").WithMessage("Password must contain an uppercase letter.")
            .Matches("[0-9]").WithMessage("Password must contain a digit.")
            .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain a special character.");
    }
}

public class RegisterTenantCommandHandler : IRequestHandler<RegisterTenantCommand, Result<AuthResponseDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IAuthTokenIssuer _tokenIssuer;

    public RegisterTenantCommandHandler(
        IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager, IAuthTokenIssuer tokenIssuer)
    {
        _unitOfWork = unitOfWork;
        _userManager = userManager;
        _tokenIssuer = tokenIssuer;
    }

    public async Task<Result<AuthResponseDto>> Handle(RegisterTenantCommand request, CancellationToken cancellationToken)
    {
        var slugTaken = await _unitOfWork.Repository<Tenant>().Query()
            .AnyAsync(t => t.Slug == request.Slug, cancellationToken);
        if (slugTaken)
        {
            return Result<AuthResponseDto>.Failure($"The clinic identifier '{request.Slug}' is already taken.");
        }

        if (await _userManager.FindByEmailAsync(request.OwnerEmail) is not null)
        {
            return Result<AuthResponseDto>.Failure("An account with this email already exists.");
        }

        var freePlan = await _unitOfWork.Repository<Plan>().Query()
            .FirstOrDefaultAsync(p => p.Tier == PlanTier.Free && p.IsActive, cancellationToken);
        if (freePlan is null)
        {
            return Result<AuthResponseDto>.Failure("Signup is temporarily unavailable — no plan is configured. Please contact support.");
        }

        var tenant = new Tenant
        {
            Name = request.ClinicName,
            Slug = request.Slug,
            ContactEmail = request.OwnerEmail,
            IsActive = true
        };
        await _unitOfWork.Repository<Tenant>().AddAsync(tenant, cancellationToken);

        var subscription = new Subscription
        {
            TenantId = tenant.Id,
            PlanId = freePlan.Id,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(freePlan.DurationInDays),
            Status = SubscriptionStatus.Trialing,
            AutoRenew = false
        };
        await _unitOfWork.Repository<Subscription>().AddAsync(subscription, cancellationToken);

        // Persist the tenant before creating its owner user, which has a FK to TenantId.
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var owner = new ApplicationUser
        {
            TenantId = tenant.Id,
            UserName = request.OwnerEmail,
            Email = request.OwnerEmail,
            EmailConfirmed = true, // no email-verification pipeline configured yet — see README for production hardening notes
            FullName = request.OwnerFullName,
            IsActive = true
        };

        var createResult = await _userManager.CreateAsync(owner, request.Password);
        if (!createResult.Succeeded)
        {
            return Result<AuthResponseDto>.Failure(createResult.Errors.Select(e => e.Description).ToArray());
        }

        await _userManager.AddToRoleAsync(owner, Roles.ClinicOwner);

        var response = await _tokenIssuer.IssueTokensAsync(owner, new[] { Roles.ClinicOwner }, request.IpAddress, cancellationToken);
        return Result<AuthResponseDto>.Success(response);
    }
}
