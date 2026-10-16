using AutoMapper.QueryableExtensions;
using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Mappings;
using ClinicManagement.Application.Features.Appointments.DTOs;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;
using ClinicManagement.Shared.Common;
using MediatR;

namespace ClinicManagement.Application.Features.Appointments.Queries.GetAppointmentsList;

public record GetAppointmentsListQuery : IRequest<PaginatedList<AppointmentDto>>
{
    public Guid? DoctorId { get; init; }
    public Guid? PatientId { get; init; }
    public Guid? BranchId { get; init; }
    public AppointmentStatus? Status { get; init; }
    public DateTime? FromDate { get; init; }
    public DateTime? ToDate { get; init; }

    public string? SortBy { get; init; } = "ScheduledStart";
    public bool SortDescending { get; init; }

    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

public class GetAppointmentsListQueryHandler : IRequestHandler<GetAppointmentsListQuery, PaginatedList<AppointmentDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly AutoMapper.IMapper _mapper;

    public GetAppointmentsListQueryHandler(IUnitOfWork unitOfWork, AutoMapper.IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PaginatedList<AppointmentDto>> Handle(GetAppointmentsListQuery request, CancellationToken cancellationToken)
    {
        var query = _unitOfWork.Repository<Appointment>().Query();

        if (request.DoctorId.HasValue) query = query.Where(a => a.DoctorId == request.DoctorId);
        if (request.PatientId.HasValue) query = query.Where(a => a.PatientId == request.PatientId);
        if (request.BranchId.HasValue) query = query.Where(a => a.BranchId == request.BranchId);
        if (request.Status.HasValue) query = query.Where(a => a.Status == request.Status);
        if (request.FromDate.HasValue) query = query.Where(a => a.ScheduledStart >= request.FromDate);
        if (request.ToDate.HasValue) query = query.Where(a => a.ScheduledStart <= request.ToDate);

        query = request.SortBy?.ToLowerInvariant() switch
        {
            "status" => request.SortDescending ? query.OrderByDescending(a => a.Status) : query.OrderBy(a => a.Status),
            "createdat" => request.SortDescending ? query.OrderByDescending(a => a.CreatedAt) : query.OrderBy(a => a.CreatedAt),
            _ => request.SortDescending
                ? query.OrderByDescending(a => a.ScheduledStart)
                : query.OrderBy(a => a.ScheduledStart)
        };

        return await query
            .ProjectTo<AppointmentDto>(_mapper.ConfigurationProvider)
            .ToPaginatedListAsync(request.PageNumber, request.PageSize);
    }
}
