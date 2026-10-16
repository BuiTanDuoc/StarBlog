using AutoMapper.QueryableExtensions;
using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Features.Departments.DTOs;
using ClinicManagement.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Application.Features.Departments.Queries.GetDepartmentsList;

public record GetDepartmentsListQuery(Guid? BranchId) : IRequest<List<DepartmentDto>>;

public class GetDepartmentsListQueryHandler : IRequestHandler<GetDepartmentsListQuery, List<DepartmentDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly AutoMapper.IMapper _mapper;

    public GetDepartmentsListQueryHandler(IUnitOfWork unitOfWork, AutoMapper.IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<List<DepartmentDto>> Handle(GetDepartmentsListQuery request, CancellationToken cancellationToken)
    {
        var query = _unitOfWork.Repository<Department>().Query();
        if (request.BranchId.HasValue) query = query.Where(d => d.BranchId == request.BranchId);

        return await query
            .OrderBy(d => d.Name)
            .ProjectTo<DepartmentDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }
}
