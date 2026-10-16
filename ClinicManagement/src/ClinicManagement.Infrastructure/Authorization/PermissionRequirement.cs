using ClinicManagement.Shared.Constants;
using Microsoft.AspNetCore.Authorization;

namespace ClinicManagement.Infrastructure.Authorization;

public class PermissionRequirement : IAuthorizationRequirement
{
    public string Permission { get; }
    public PermissionRequirement(string permission) => Permission = permission;
}

/// <summary>
/// Grants access if the user has a "permission" claim matching the
/// requirement, or is in the SuperAdmin role (which bypasses all
/// permission checks — see RolePermissions.Map[SuperAdmin] being empty by
/// design). Permission claims are baked into the JWT at login by
/// TokenService, so this handler does zero DB/IO work.
/// </summary>
public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        if (context.User.IsInRole(Roles.SuperAdmin))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        var hasPermission = context.User.Claims.Any(c => c.Type == "permission" && c.Value == requirement.Permission);
        if (hasPermission)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
