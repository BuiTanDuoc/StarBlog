using AutoMapper.QueryableExtensions;
using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Mappings;
using ClinicManagement.Application.Features.Doctors.DTOs;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;
using ClinicManagement.Shared.Common;
using MediatR;

namespace ClinicManagement.Application.Features.Doctors.Queries.GetDoctorsList;

public record GetDoctorsListQuery : IRequest<PaginatedList<DoctorListItemDto>>
{
    public Guid? BranchId { get; init; }
    public string? Specialization { get; init; }
    public DoctorStatus? Status { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

public class GetDoctorsListQueryHandler : IRequestHandler<GetDoctorsListQuery, PaginatedList<DoctorListItemDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly AutoMapper.IMapper _mapper;

    public GetDoctorsListQueryHandler(IUnitOfWork unitOfWork, AutoMapper.IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PaginatedList<DoctorListItemDto>> Handle(GetDoctorsListQuery request, CancellationToken cancellationToken)
    {
        var query = _unitOfWork.Repository<Doctor>().Query();

        if (request.BranchId.HasValue) query = query.Where(d => d.BranchId == request.BranchId);
        if (!string.IsNullOrWhiteSpace(request.Specialization)) query = query.Where(d => d.Specialization.Contains(request.Specialization));
        if (request.Status.HasValue) query = query.Where(d => d.Status == request.Status);

        return await query
            .OrderBy(d => d.FullName)
            .ProjectTo<DoctorListItemDto>(_mapper.ConfigurationProvider)
            .ToPaginatedListAsync(request.PageNumber, request.PageSize);
    }
}
