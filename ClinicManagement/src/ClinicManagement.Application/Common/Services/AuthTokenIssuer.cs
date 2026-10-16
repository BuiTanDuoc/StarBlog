using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Features.Auth.DTOs;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Shared.Constants;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Application.Common.Services;

public interface IAuthTokenIssuer
{
    /// <summary>Generates a new access+refresh token pair, persists the refresh token, and returns the login response shape.</summary>
    Task<AuthResponseDto> IssueTokensAsync(
        ApplicationUser user, IList<string> roles, string? ipAddress, CancellationToken cancellationToken = default);
}

public class AuthTokenIssuer : IAuthTokenIssuer
{
    private readonly ITokenService _tokenService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly int _refreshTokenExpirationDays;

    public AuthTokenIssuer(ITokenService tokenService, IUnitOfWork unitOfWork)
    {
        _tokenService = tokenService;
        _unitOfWork = unitOfWork;
        _refreshTokenExpirationDays = 7; // mirrors Infrastructure JwtSettings default; kept here to avoid an Infra dependency
    }

    public async Task<AuthResponseDto> IssueTokensAsync(
        ApplicationUser user, IList<string> roles, string? ipAddress, CancellationToken cancellationToken = default)
    {
        // Only patient-portal logins carry a patient_id claim. Resolved here (rather than
        // requiring every caller — Login, RefreshToken, RegisterPatientAccount — to know and
        // pass it) so this is the single place that decides "is this a patient token".
        Guid? patientId = null;
        if (roles.Contains(Roles.Patient))
        {
            patientId = await _unitOfWork.Repository<Patient>().Query()
                .Where(p => p.ApplicationUserId == user.Id)
                .Select(p => (Guid?)p.Id)
                .FirstOrDefaultAsync(cancellationToken);
        }

        var accessToken = _tokenService.GenerateAccessToken(user, roles, patientId);

        var refreshToken = new RefreshToken
        {
            Token = _tokenService.GenerateRefreshTokenValue(),
            UserId = user.Id,
            CreatedByIp = ipAddress,
            ExpiresAt = DateTime.UtcNow.AddDays(_refreshTokenExpirationDays)
        };

        await _unitOfWork.Repository<RefreshToken>().AddAsync(refreshToken, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new AuthResponseDto(
            accessToken.Token,
            accessToken.ExpiresAt,
            refreshToken.Token,
            user.Id,
            user.Email ?? string.Empty,
            user.FullName,
            user.TenantId,
            roles.ToList());
    }
}
