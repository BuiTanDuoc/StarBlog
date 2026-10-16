using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Entities;
using FluentValidation;
using MediatR;

namespace ClinicManagement.Application.Features.Expenses.Commands.DeleteExpense;

public record DeleteExpenseCommand(Guid Id) : IRequest<Result>;

public class DeleteExpenseCommandValidator : AbstractValidator<DeleteExpenseCommand>
{
    public DeleteExpenseCommandValidator() => RuleFor(x => x.Id).NotEmpty();
}

public class DeleteExpenseCommandHandler : IRequestHandler<DeleteExpenseCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteExpenseCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Result> Handle(DeleteExpenseCommand request, CancellationToken cancellationToken)
    {
        var repository = _unitOfWork.Repository<Expense>();
        var expense = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (expense is null)
        {
            return Result.Failure($"Expense with id '{request.Id}' was not found.");
        }

        repository.Remove(expense);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
