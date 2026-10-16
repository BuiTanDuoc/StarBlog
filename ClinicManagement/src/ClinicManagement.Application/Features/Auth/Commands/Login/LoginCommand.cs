using ClinicManagement.Application.Common.Services;
using ClinicManagement.Application.Features.Auth.DTOs;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace ClinicManagement.Application.Features.Auth.Commands.Login;

public record LoginCommand(string Email, string Password, string? IpAddress) : IRequest<Result<AuthResponseDto>>;

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty();
    }
}

public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<AuthResponseDto>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IAuthTokenIssuer _tokenIssuer;

    public LoginCommandHandler(UserManager<ApplicationUser> userManager, IAuthTokenIssuer tokenIssuer)
    {
        _userManager = userManager;
        _tokenIssuer = tokenIssuer;
    }

    public async Task<Result<AuthResponseDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);

        // Same generic failure message whether the email doesn't exist or the password is
        // wrong — don't leak which one it was.
        const string genericError = "Invalid email or password.";

        if (user is null || !user.IsActive)
        {
            return Result<AuthResponseDto>.Failure(genericError);
        }

        if (await _userManager.IsLockedOutAsync(user))
        {
            return Result<AuthResponseDto>.Failure("This account is temporarily locked due to too many failed attempts. Try again later.");
        }

        var passwordValid = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!passwordValid)
        {
            await _userManager.AccessFailedAsync(user); // increments the lockout counter
            return Result<AuthResponseDto>.Failure(genericError);
        }

        await _userManager.ResetAccessFailedCountAsync(user);

        user.LastLoginAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        var roles = await _userManager.GetRolesAsync(user);
        var response = await _tokenIssuer.IssueTokensAsync(user, roles, request.IpAddress, cancellationToken);

        return Result<AuthResponseDto>.Success(response);
    }
}
