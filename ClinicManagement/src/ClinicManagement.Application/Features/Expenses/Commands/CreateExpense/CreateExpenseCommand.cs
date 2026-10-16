using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Features.Expenses.DTOs;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;
using FluentValidation;
using MediatR;

namespace ClinicManagement.Application.Features.Expenses.Commands.CreateExpense;

public record CreateExpenseCommand : IRequest<Result<ExpenseDto>>
{
    public Guid? BranchId { get; init; }
    public string Category { get; init; } = default!;
    public string? Description { get; init; }
    public decimal Amount { get; init; }
    public DateTime ExpenseDate { get; init; }
    public PaymentMethod Method { get; init; }
}

public class CreateExpenseCommandValidator : AbstractValidator<CreateExpenseCommand>
{
    public CreateExpenseCommandValidator()
    {
        RuleFor(x => x.Category).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.ExpenseDate).NotEmpty();
    }
}

public class CreateExpenseCommandHandler : IRequestHandler<CreateExpenseCommand, Result<ExpenseDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly AutoMapper.IMapper _mapper;

    public CreateExpenseCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService, AutoMapper.IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _mapper = mapper;
    }

    public async Task<Result<ExpenseDto>> Handle(CreateExpenseCommand request, CancellationToken cancellationToken)
    {
        var expense = new Expense
        {
            TenantId = _currentUserService.TenantId,
            BranchId = request.BranchId,
            Category = request.Category,
            Description = request.Description,
            Amount = request.Amount,
            ExpenseDate = request.ExpenseDate,
            Method = request.Method,
            ApprovedBy = _currentUserService.UserName
        };

        await _unitOfWork.Repository<Expense>().AddAsync(expense, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<ExpenseDto>.Success(_mapper.Map<ExpenseDto>(expense));
    }
}
