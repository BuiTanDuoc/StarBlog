using AutoMapper.QueryableExtensions;
using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Features.Appointments.DTOs;
using ClinicManagement.Application.Features.Dashboard.DTOs;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Application.Features.Dashboard.Queries.GetDoctorDashboard;

public record GetDoctorDashboardQuery : IRequest<DoctorDashboardDto>;

/// <summary>Resolves "which doctor" from the caller's own ApplicationUserId — there's no
/// DoctorId parameter, so a doctor can only ever see their own dashboard through this endpoint.</summary>
public class GetDoctorDashboardQueryHandler : IRequestHandler<GetDoctorDashboardQuery, DoctorDashboardDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly AutoMapper.IMapper _mapper;

    public GetDoctorDashboardQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService, AutoMapper.IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _mapper = mapper;
    }

    public async Task<DoctorDashboardDto> Handle(GetDoctorDashboardQuery request, CancellationToken cancellationToken)
    {
        var doctor = await _unitOfWork.Repository<Doctor>().Query()
            .FirstOrDefaultAsync(d => d.ApplicationUserId == _currentUserService.UserId, cancellationToken);

        if (doctor is null)
        {
            // Logged in with the Doctor role but no linked Doctor profile yet (e.g. account
            // created via RegisterUserCommand before the profile record was filled in).
            return new DoctorDashboardDto(new List<AppointmentDto>(), new List<AppointmentDto>());
        }

        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);
        var weekAhead = today.AddDays(8);

        var todaysAppointments = await _unitOfWork.Repository<Appointment>().Query()
            .Where(a => a.DoctorId == doctor.Id && a.ScheduledStart >= today && a.ScheduledStart < tomorrow)
            .OrderBy(a => a.ScheduledStart)
            .ProjectTo<AppointmentDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        var upcomingAppointments = await _unitOfWork.Repository<Appointment>().Query()
            .Where(a => a.DoctorId == doctor.Id
                        && a.ScheduledStart >= tomorrow && a.ScheduledStart < weekAhead
                        && a.Status != AppointmentStatus.Cancelled && a.Status != AppointmentStatus.NoShow)
            .OrderBy(a => a.ScheduledStart)
            .Take(10)
            .ProjectTo<AppointmentDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        return new DoctorDashboardDto(todaysAppointments, upcomingAppointments);
    }
}
