using AutoMapper.QueryableExtensions;
using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Mappings;
using ClinicManagement.Application.Features.Expenses.DTOs;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Shared.Common;
using MediatR;

namespace ClinicManagement.Application.Features.Expenses.Queries.GetExpensesList;

public record GetExpensesListQuery : IRequest<PaginatedList<ExpenseDto>>
{
    public DateTime? FromDate { get; init; }
    public DateTime? ToDate { get; init; }
    public string? Category { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

public class GetExpensesListQueryHandler : IRequestHandler<GetExpensesListQuery, PaginatedList<ExpenseDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly AutoMapper.IMapper _mapper;

    public GetExpensesListQueryHandler(IUnitOfWork unitOfWork, AutoMapper.IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PaginatedList<ExpenseDto>> Handle(GetExpensesListQuery request, CancellationToken cancellationToken)
    {
        var query = _unitOfWork.Repository<Expense>().Query();

        if (request.FromDate.HasValue) query = query.Where(e => e.ExpenseDate >= request.FromDate);
        if (request.ToDate.HasValue) query = query.Where(e => e.ExpenseDate <= request.ToDate);
        if (!string.IsNullOrWhiteSpace(request.Category)) query = query.Where(e => e.Category == request.Category);

        return await query
            .OrderByDescending(e => e.ExpenseDate)
            .ProjectTo<ExpenseDto>(_mapper.ConfigurationProvider)
            .ToPaginatedListAsync(request.PageNumber, request.PageSize);
    }
}
