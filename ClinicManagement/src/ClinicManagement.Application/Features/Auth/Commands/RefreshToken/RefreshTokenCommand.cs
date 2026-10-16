using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Services;
using ClinicManagement.Application.Features.Auth.DTOs;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Application.Features.Auth.Commands.RefreshToken;

public record RefreshTokenCommand(string RefreshToken, string? IpAddress) : IRequest<Result<AuthResponseDto>>;

public class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(x => x.RefreshToken).NotEmpty();
    }
}

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, Result<AuthResponseDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IAuthTokenIssuer _tokenIssuer;

    public RefreshTokenCommandHandler(
        IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager, IAuthTokenIssuer tokenIssuer)
    {
        _unitOfWork = unitOfWork;
        _userManager = userManager;
        _tokenIssuer = tokenIssuer;
    }

    public async Task<Result<AuthResponseDto>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var repository = _unitOfWork.Repository<ClinicManagement.Domain.Entities.RefreshToken>();

        var existingToken = await repository.Query()
            .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken, cancellationToken);

        if (existingToken is null)
        {
            return Result<AuthResponseDto>.Failure("Invalid refresh token.");
        }

        if (!existingToken.IsActive)
        {
            // Reused a revoked/expired token — this can indicate token theft. In a fuller
            // implementation this would revoke the entire token family for the user; flagged
            // here as the next hardening step rather than silently no-opping.
            return Result<AuthResponseDto>.Failure("Refresh token is no longer valid. Please log in again.");
        }

        var user = await _userManager.FindByIdAsync(existingToken.UserId.ToString());
        if (user is null || !user.IsActive)
        {
            return Result<AuthResponseDto>.Failure("Account is no longer active.");
        }

        var roles = await _userManager.GetRolesAsync(user);
        var response = await _tokenIssuer.IssueTokensAsync(user, roles, request.IpAddress, cancellationToken);

        // Rotate: revoke the old token and link it forward to the new one.
        existingToken.RevokedAt = DateTime.UtcNow;
        existingToken.RevokedByIp = request.IpAddress;
        existingToken.ReplacedByToken = response.RefreshToken;
        repository.Update(existingToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<AuthResponseDto>.Success(response);
    }
}
