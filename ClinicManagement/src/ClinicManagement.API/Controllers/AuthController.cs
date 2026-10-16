using ClinicManagement.API.Common;
using ClinicManagement.Application.Features.Auth.Commands.ChangePassword;
using ClinicManagement.Application.Features.Auth.Commands.Login;
using ClinicManagement.Application.Features.Auth.Commands.RefreshToken;
using ClinicManagement.Application.Features.Auth.Commands.RegisterTenant;
using ClinicManagement.Application.Features.Auth.Commands.RegisterUser;
using ClinicManagement.Application.Features.Auth.Commands.RevokeToken;
using ClinicManagement.Shared.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagement.API.Controllers;

public class AuthController : ApiControllerBase
{
    /// <summary>Self-service clinic signup — creates a new Tenant, its ClinicOwner user, and a Free trial Subscription.</summary>
    [AllowAnonymous]
    [HttpPost("register-tenant")]
    public async Task<IActionResult> RegisterTenant(RegisterTenantCommand command)
    {
        var result = await Mediator.Send(command with { IpAddress = ClientIpAddress });
        return HandleResult(result);
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginCommand command)
    {
        var result = await Mediator.Send(command with { IpAddress = ClientIpAddress });
        return HandleResult(result);
    }

    [AllowAnonymous]
    [HttpPost("refresh-token")]
    public async Task<IActionResult> Refresh(RefreshTokenCommand command)
    {
        var result = await Mediator.Send(command with { IpAddress = ClientIpAddress });
        return HandleResult(result);
    }

    /// <summary>Explicit logout — revokes the given refresh token so it can no longer be used to mint new access tokens.</summary>
    [AllowAnonymous]
    [HttpPost("revoke-token")]
    public async Task<IActionResult> Revoke(RevokeTokenCommand command)
    {
        var result = await Mediator.Send(command with { IpAddress = ClientIpAddress });
        return HandleResult(result);
    }

    /// <summary>Invite staff into your own clinic. ClinicOwner only.</summary>
    [Authorize(Roles = Roles.ClinicOwner)]
    [HttpPost("register-user")]
    public async Task<IActionResult> RegisterUser(RegisterUserCommand command)
    {
        var result = await Mediator.Send(command);
        return HandleResult(result);
    }

    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword(ChangePasswordCommand command)
    {
        var result = await Mediator.Send(command);
        return HandleResult(result);
    }
}
