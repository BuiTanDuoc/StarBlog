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

namespace ClinicManagement.Application.Features.PatientPortal.Commands.RegisterPatientAccount;

/// <summary>
/// Public self-service signup for the patient portal — a patient picks their
/// clinic by its public Slug (e.g. the URL "/portal/{slug}"), and this
/// always creates a brand-new Patient row for that tenant rather than
/// trying to "claim" an existing reception-created record by matching
/// phone/national ID: auto-linking by phone would let anyone who knows a
/// patient's phone number register and gain access to that patient's
/// medical history. A clinic that wants to link a walk-in patient's
/// existing chart to a new portal login should do that explicitly from the
/// staff side (a follow-up feature, not implemented here) rather than
/// trusting whatever the person typed into the public signup form.
/// </summary>
public record RegisterPatientAccountCommand : IRequest<Result<AuthResponseDto>>
{
    public string TenantSlug { get; init; } = default!;
    public string FullName { get; init; } = default!;
    public string Email { get; init; } = default!;
    public string Phone { get; init; } = default!;
    public string Password { get; init; } = default!;
    public DateTime DateOfBirth { get; init; }
    public Gender Gender { get; init; }
    public string? IpAddress { get; init; }
}

public class RegisterPatientAccountCommandValidator : AbstractValidator<RegisterPatientAccountCommand>
{
    public RegisterPatientAccountCommandValidator()
    {
        RuleFor(x => x.TenantSlug).NotEmpty().MaximumLength(100);
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Phone).NotEmpty().MaximumLength(30);
        RuleFor(x => x.DateOfBirth).NotEmpty().LessThan(DateTime.UtcNow).WithMessage("Date of birth must be in the past.");
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8)
            .Matches("[A-Z]").WithMessage("Password must contain an uppercase letter.")
            .Matches("[0-9]").WithMessage("Password must contain a digit.")
            .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain a special character.");
    }
}

public class RegisterPatientAccountCommandHandler : IRequestHandler<RegisterPatientAccountCommand, Result<AuthResponseDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IAuthTokenIssuer _tokenIssuer;

    public RegisterPatientAccountCommandHandler(
        IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager, IAuthTokenIssuer tokenIssuer)
    {
        _unitOfWork = unitOfWork;
        _userManager = userManager;
        _tokenIssuer = tokenIssuer;
    }

    public async Task<Result<AuthResponseDto>> Handle(RegisterPatientAccountCommand request, CancellationToken cancellationToken)
    {
        // Tenant is not itself tenant-scoped (it's the tenant root), so this lookup needs no
        // IgnoreQueryFilters — unlike almost every other query in this handler would if it
        // touched a Patient/Appointment/etc. table before a tenant context exists.
        var tenant = await _unitOfWork.Repository<Tenant>().Query()
            .FirstOrDefaultAsync(t => t.Slug == request.TenantSlug && t.IsActive, cancellationToken);

        if (tenant is null)
        {
            return Result<AuthResponseDto>.Failure($"No active clinic found for '{request.TenantSlug}'.");
        }

        if (await _userManager.FindByEmailAsync(request.Email) is not null)
        {
            return Result<AuthResponseDto>.Failure("An account with this email already exists.");
        }

        var patient = new Patient
        {
            TenantId = tenant.Id,
            PatientCode = await GenerateNextPatientCodeAsync(tenant.Id, cancellationToken),
            FullName = request.FullName,
            Gender = request.Gender,
            DateOfBirth = request.DateOfBirth,
            Phone = request.Phone,
            Email = request.Email,
        };
        await _unitOfWork.Repository<Patient>().AddAsync(patient, cancellationToken);

        // Persist the patient before creating its portal login, which has an FK to PatientId
        // — same "parent row first" ordering RegisterTenantCommand uses for Tenant/owner.
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var user = new ApplicationUser
        {
            TenantId = tenant.Id,
            UserName = request.Email,
            Email = request.Email,
            EmailConfirmed = true, // no email-verification pipeline configured yet — same trade-off as RegisterTenantCommand
            FullName = request.FullName,
            IsActive = true
        };

        var createResult = await _userManager.CreateAsync(user, request.Password);
        if (!createResult.Succeeded)
        {
            return Result<AuthResponseDto>.Failure(createResult.Errors.Select(e => e.Description).ToArray());
        }

        await _userManager.AddToRoleAsync(user, Roles.Patient);

        patient.ApplicationUserId = user.Id;
        _unitOfWork.Repository<Patient>().Update(patient);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var response = await _tokenIssuer.IssueTokensAsync(user, new[] { Roles.Patient }, request.IpAddress, cancellationToken);
        return Result<AuthResponseDto>.Success(response);
    }

    /// <summary>"PT-000123" — see the identical trade-off note on CreatePatientCommand. IgnoreQueryFilters is required here because the caller has no tenant context yet (this runs before login).</summary>
    private async Task<string> GenerateNextPatientCodeAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        var count = await _unitOfWork.Repository<Patient>().Query()
            .IgnoreQueryFilters()
            .Where(p => p.TenantId == tenantId)
            .CountAsync(cancellationToken);

        return $"PT-{(count + 1):D6}";
    }
}
