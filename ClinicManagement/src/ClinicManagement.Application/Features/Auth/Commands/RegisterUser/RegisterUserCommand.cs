using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Shared.Constants;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace ClinicManagement.Application.Features.Auth.Commands.RegisterUser;

/// <summary>
/// Creates a staff account (Doctor, Receptionist, Accountant, or Nurse)
/// within the caller's own tenant. Gated by [HasPermission] at the
/// controller (Phase 6) — typically only ClinicOwner can invite staff.
/// Does not log the new user in; they receive their own credentials and
/// call Login separately. Does not create the linked Doctor/Employee
/// profile row — that's a separate step in the Doctors/Employees feature
/// so an owner can register the login before or after filling in
/// specialization/salary/etc.
/// </summary>
public record RegisterUserCommand : IRequest<Result<Guid>>
{
    public string FullName { get; init; } = default!;
    public string Email { get; init; } = default!;
    public string Password { get; init; } = default!;
    public string Role { get; init; } = default!;
}

public class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    private static readonly string[] AssignableRoles =
    {
        Roles.Doctor, Roles.Receptionist, Roles.Accountant, Roles.Nurse, Roles.ClinicOwner
    };

    public RegisterUserCommandValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8);
        RuleFor(x => x.Role).NotEmpty()
            .Must(r => AssignableRoles.Contains(r))
            .WithMessage($"Role must be one of: {string.Join(", ", AssignableRoles)}. SuperAdmin cannot be assigned this way.");
    }
}

public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, Result<Guid>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ICurrentUserService _currentUserService;

    public RegisterUserCommandHandler(UserManager<ApplicationUser> userManager, ICurrentUserService currentUserService)
    {
        _userManager = userManager;
        _currentUserService = currentUserService;
    }

    public async Task<Result<Guid>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        if (await _userManager.FindByEmailAsync(request.Email) is not null)
        {
            return Result<Guid>.Failure("An account with this email already exists.");
        }

        var user = new ApplicationUser
        {
            TenantId = _currentUserService.TenantId,
            UserName = request.Email,
            Email = request.Email,
            EmailConfirmed = true,
            FullName = request.FullName,
            IsActive = true
        };

        var createResult = await _userManager.CreateAsync(user, request.Password);
        if (!createResult.Succeeded)
        {
            return Result<Guid>.Failure(createResult.Errors.Select(e => e.Description).ToArray());
        }

        await _userManager.AddToRoleAsync(user, request.Role);

        return Result<Guid>.Success(user.Id);
    }
}
