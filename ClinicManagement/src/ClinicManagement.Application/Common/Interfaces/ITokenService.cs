using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Application.Common.Interfaces;

public record AccessTokenResult(string Token, DateTime ExpiresAt);

/// <summary>
/// Issues JWT access tokens (short-lived, carry tenant_id/role/permission
/// claims) and opaque refresh tokens (long-lived, DB-backed, rotated on
/// each use). Implemented in Infrastructure/Identity/TokenService.cs.
/// </summary>
public interface ITokenService
{
    /// <summary>patientId is baked in as a "patient_id" claim when the token belongs to a patient-portal login — see ICurrentUserService.PatientId.</summary>
    AccessTokenResult GenerateAccessToken(ApplicationUser user, IList<string> roles, Guid? patientId = null);

    /// <summary>Cryptographically random opaque string — not a JWT, just a lookup key stored in the RefreshToken table.</summary>
    string GenerateRefreshTokenValue();
}
