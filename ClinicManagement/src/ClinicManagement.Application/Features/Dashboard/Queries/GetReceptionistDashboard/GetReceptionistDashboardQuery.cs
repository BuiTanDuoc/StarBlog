using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Features.Dashboard.DTOs;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Application.Features.Dashboard.Queries.GetReceptionistDashboard;

public record GetReceptionistDashboardQuery : IRequest<ReceptionistDashboardDto>;

public class GetReceptionistDashboardQueryHandler : IRequestHandler<GetReceptionistDashboardQuery, ReceptionistDashboardDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetReceptionistDashboardQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ReceptionistDashboardDto> Handle(GetReceptionistDashboardQuery request, CancellationToken cancellationToken)
    {
        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);

        var query = _unitOfWork.Repository<Appointment>().Query();

        // "Bookings today" = new appointments created today, regardless of which day they're
        // scheduled for — distinct from "scheduled today", which AppointmentsToday on the
        // ClinicOwner dashboard already covers.
        var bookingsToday = await query
            .CountAsync(a => a.CreatedAt >= today && a.CreatedAt < tomorrow, cancellationToken);

        var checkInsToday = await query
            .CountAsync(a => a.ScheduledStart >= today && a.ScheduledStart < tomorrow
                              && (a.Status == AppointmentStatus.CheckedIn || a.Status == AppointmentStatus.InProgress
                                  || a.Status == AppointmentStatus.Completed),
                cancellationToken);

        // Currently sitting in the waiting room right now — checked in but not yet called in.
        var waitingPatients = await query
            .CountAsync(a => a.Status == AppointmentStatus.CheckedIn, cancellationToken);

        return new ReceptionistDashboardDto(bookingsToday, checkInsToday, waitingPatients);
    }
}
