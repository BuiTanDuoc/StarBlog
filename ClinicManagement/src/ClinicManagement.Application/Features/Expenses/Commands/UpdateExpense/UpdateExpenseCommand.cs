using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Features.Expenses.DTOs;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;
using FluentValidation;
using MediatR;

namespace ClinicManagement.Application.Features.Expenses.Commands.UpdateExpense;

public record UpdateExpenseCommand : IRequest<Result<ExpenseDto>>
{
    public Guid Id { get; init; }
    public string Category { get; init; } = default!;
    public string? Description { get; init; }
    public decimal Amount { get; init; }
    public DateTime ExpenseDate { get; init; }
    public PaymentMethod Method { get; init; }
}

public class UpdateExpenseCommandValidator : AbstractValidator<UpdateExpenseCommand>
{
    public UpdateExpenseCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Category).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Amount).GreaterThan(0);
    }
}

public class UpdateExpenseCommandHandler : IRequestHandler<UpdateExpenseCommand, Result<ExpenseDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly AutoMapper.IMapper _mapper;

    public UpdateExpenseCommandHandler(IUnitOfWork unitOfWork, AutoMapper.IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<ExpenseDto>> Handle(UpdateExpenseCommand request, CancellationToken cancellationToken)
    {
        var repository = _unitOfWork.Repository<Expense>();
        var expense = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (expense is null)
        {
            return Result<ExpenseDto>.Failure($"Expense with id '{request.Id}' was not found.");
        }

        expense.Category = request.Category;
        expense.Description = request.Description;
        expense.Amount = request.Amount;
        expense.ExpenseDate = request.ExpenseDate;
        expense.Method = request.Method;

        repository.Update(expense);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<ExpenseDto>.Success(_mapper.Map<ExpenseDto>(expense));
    }
}
