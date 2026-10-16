using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Features.Reports.DTOs;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Application.Features.Reports.Queries.GetAppointmentsReport;

public record GetAppointmentsReportQuery(DateTime? FromDate, DateTime? ToDate, AppointmentStatus? Status)
    : IRequest<List<AppointmentsReportRowDto>>;

public class GetAppointmentsReportQueryHandler
    : IRequestHandler<GetAppointmentsReportQuery, List<AppointmentsReportRowDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAppointmentsReportQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<AppointmentsReportRowDto>> Handle(GetAppointmentsReportQuery request, CancellationToken cancellationToken)
    {
        var query = _unitOfWork.Repository<Appointment>().Query();

        if (request.FromDate.HasValue) query = query.Where(a => a.ScheduledStart >= request.FromDate);
        if (request.ToDate.HasValue) query = query.Where(a => a.ScheduledStart <= request.ToDate);
        if (request.Status.HasValue) query = query.Where(a => a.Status == request.Status);

        return await query
            .OrderBy(a => a.ScheduledStart)
            .Select(a => new AppointmentsReportRowDto(
                a.ScheduledStart,
                a.Patient.FullName,
                a.Doctor.FullName,
                a.Branch.Name,
                a.Status.ToString()))
            .ToListAsync(cancellationToken);
    }
}
