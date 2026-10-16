using AutoMapper.QueryableExtensions;
using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Features.Appointments.DTOs;
using ClinicManagement.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Application.Features.Appointments.Queries.GetAppointmentTimeline;

public record GetAppointmentTimelineQuery(Guid AppointmentId) : IRequest<List<AppointmentStatusHistoryDto>>;

public class GetAppointmentTimelineQueryHandler
    : IRequestHandler<GetAppointmentTimelineQuery, List<AppointmentStatusHistoryDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly AutoMapper.IMapper _mapper;

    public GetAppointmentTimelineQueryHandler(IUnitOfWork unitOfWork, AutoMapper.IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<List<AppointmentStatusHistoryDto>> Handle(GetAppointmentTimelineQuery request, CancellationToken cancellationToken)
    {
        return await _unitOfWork.Repository<AppointmentStatusHistory>().Query()
            .Where(h => h.AppointmentId == request.AppointmentId)
            .OrderBy(h => h.ChangedAt)
            .ProjectTo<AppointmentStatusHistoryDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }
}
