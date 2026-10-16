using AutoMapper.QueryableExtensions;
using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Mappings;
using ClinicManagement.Application.Features.Employees.DTOs;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Shared.Common;
using MediatR;

namespace ClinicManagement.Application.Features.Employees.Queries.GetEmployeesList;

public record GetEmployeesListQuery : IRequest<PaginatedList<EmployeeListItemDto>>
{
    public Guid? BranchId { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

public class GetEmployeesListQueryHandler : IRequestHandler<GetEmployeesListQuery, PaginatedList<EmployeeListItemDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly AutoMapper.IMapper _mapper;

    public GetEmployeesListQueryHandler(IUnitOfWork unitOfWork, AutoMapper.IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PaginatedList<EmployeeListItemDto>> Handle(GetEmployeesListQuery request, CancellationToken cancellationToken)
    {
        var query = _unitOfWork.Repository<Employee>().Query();
        if (request.BranchId.HasValue) query = query.Where(e => e.BranchId == request.BranchId);

        return await query
            .OrderBy(e => e.FullName)
            .ProjectTo<EmployeeListItemDto>(_mapper.ConfigurationProvider)
            .ToPaginatedListAsync(request.PageNumber, request.PageSize);
    }
}
