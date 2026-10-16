using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Application.Features.Auth.Commands.RevokeToken;

public record RevokeTokenCommand(string RefreshToken, string? IpAddress) : IRequest<Result>;

public class RevokeTokenCommandValidator : AbstractValidator<RevokeTokenCommand>
{
    public RevokeTokenCommandValidator()
    {
        RuleFor(x => x.RefreshToken).NotEmpty();
    }
}

public class RevokeTokenCommandHandler : IRequestHandler<RevokeTokenCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;

    public RevokeTokenCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(RevokeTokenCommand request, CancellationToken cancellationToken)
    {
        var repository = _unitOfWork.Repository<ClinicManagement.Domain.Entities.RefreshToken>();
        var token = await repository.Query().FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken, cancellationToken);

        if (token is null || !token.IsActive)
        {
            return Result.Failure("Token not found or already inactive.");
        }

        token.RevokedAt = DateTime.UtcNow;
        token.RevokedByIp = request.IpAddress;
        repository.Update(token);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
