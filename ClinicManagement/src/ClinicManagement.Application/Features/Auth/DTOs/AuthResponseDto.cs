namespace ClinicManagement.Application.Features.Auth.DTOs;

public record AuthResponseDto(
    string AccessToken,
    DateTime AccessTokenExpiresAt,
    string RefreshToken,
    Guid UserId,
    string Email,
    string FullName,
    Guid TenantId,
    IReadOnlyList<string> Roles);
