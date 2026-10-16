using System.Security.Claims;
using ClinicManagement.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;
using RoleConstants = ClinicManagement.Shared.Constants.Roles;

namespace ClinicManagement.Infrastructure.Services;

/// <summary>
/// Reads the authenticated user's identity from the JWT claims on the
/// current HTTP request. This is what every tenant-isolation check
/// ultimately traces back to — see TokenService (Phase 4) for where these
/// claims ("tenant_id", "sub", roles) get put into the token in the first
/// place.
/// </summary>
public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;

    public Guid? UserId
    {
        get
        {
            var value = User?.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(value, out var id) ? id : null;
        }
    }

    public Guid TenantId
    {
        get
        {
            var value = User?.FindFirstValue("tenant_id");
            return Guid.TryParse(value, out var id) ? id : Guid.Empty;
        }
    }

    public string? UserName => User?.FindFirstValue(ClaimTypes.Email) ?? User?.Identity?.Name;

    public bool IsSuperAdmin => Roles.Contains(RoleConstants.SuperAdmin);

    public Guid? PatientId
    {
        get
        {
            var value = User?.FindFirstValue("patient_id");
            return Guid.TryParse(value, out var id) ? id : null;
        }
    }

    public IReadOnlyList<string> Roles =>
        User?.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList() ?? new List<string>();
}
