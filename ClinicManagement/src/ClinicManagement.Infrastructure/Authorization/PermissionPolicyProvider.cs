using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace ClinicManagement.Infrastructure.Authorization;

/// <summary>
/// Registered in place of the default policy provider (API Program.cs,
/// Phase 6). Any policy name of the form "Permission:X" is satisfied by
/// constructing a PermissionRequirement("X") on the fly, so [HasPermission]
/// attributes work without a matching services.AddAuthorization(options =>
/// options.AddPolicy(...)) call for every single permission string.
/// </summary>
public class PermissionPolicyProvider : IAuthorizationPolicyProvider
{
    private const string PolicyPrefix = "Permission:";
    private readonly DefaultAuthorizationPolicyProvider _fallbackProvider;

    public PermissionPolicyProvider(IOptions<AuthorizationOptions> options)
    {
        _fallbackProvider = new DefaultAuthorizationPolicyProvider(options);
    }

    public Task<AuthorizationPolicy> GetDefaultPolicyAsync() => _fallbackProvider.GetDefaultPolicyAsync();

    public Task<AuthorizationPolicy?> GetFallbackPolicyAsync() => _fallbackProvider.GetFallbackPolicyAsync();

    public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        if (policyName.StartsWith(PolicyPrefix, StringComparison.OrdinalIgnoreCase))
        {
            var permission = policyName[PolicyPrefix.Length..];
            var policy = new AuthorizationPolicyBuilder()
                .AddRequirements(new PermissionRequirement(permission))
                .Build();

            return Task.FromResult<AuthorizationPolicy?>(policy);
        }

        return _fallbackProvider.GetPolicyAsync(policyName);
    }
}
