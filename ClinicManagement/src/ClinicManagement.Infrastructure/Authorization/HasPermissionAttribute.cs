using Microsoft.AspNetCore.Authorization;

namespace ClinicManagement.Infrastructure.Authorization;

/// <summary>
/// Usage on a controller action: [HasPermission(Permissions.Patients.Delete)].
/// Resolved dynamically by PermissionPolicyProvider — no need to register
/// each permission string as a named policy at startup.
/// </summary>
public class HasPermissionAttribute : AuthorizeAttribute
{
    public HasPermissionAttribute(string permission) : base(policy: $"Permission:{permission}")
    {
    }
}
