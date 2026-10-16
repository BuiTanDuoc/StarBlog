using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Entities;
using FluentValidation;
using MediatR;

namespace ClinicManagement.Application.Features.Branches.Commands.DeleteBranch;

public record DeleteBranchCommand(Guid Id) : IRequest<Result>;

public class DeleteBranchCommandValidator : AbstractValidator<DeleteBranchCommand>
{
    public DeleteBranchCommandValidator() => RuleFor(x => x.Id).NotEmpty();
}

public class DeleteBranchCommandHandler : IRequestHandler<DeleteBranchCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteBranchCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Result> Handle(DeleteBranchCommand request, CancellationToken cancellationToken)
    {
        var repository = _unitOfWork.Repository<Branch>();
        var branch = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (branch is null)
        {
            return Result.Failure($"Branch with id '{request.Id}' was not found.");
        }

        repository.Remove(branch);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
