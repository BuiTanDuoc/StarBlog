using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Features.Reports.DTOs;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Application.Features.Reports.Queries.GetDoctorPerformanceReport;

public record GetDoctorPerformanceReportQuery(DateTime? FromDate, DateTime? ToDate)
    : IRequest<List<DoctorPerformanceReportRowDto>>;

public class GetDoctorPerformanceReportQueryHandler
    : IRequestHandler<GetDoctorPerformanceReportQuery, List<DoctorPerformanceReportRowDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetDoctorPerformanceReportQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<DoctorPerformanceReportRowDto>> Handle(
        GetDoctorPerformanceReportQuery request, CancellationToken cancellationToken)
    {
        var appointmentsQuery = _unitOfWork.Repository<Appointment>().Query();

        if (request.FromDate.HasValue) appointmentsQuery = appointmentsQuery.Where(a => a.ScheduledStart >= request.FromDate);
        if (request.ToDate.HasValue) appointmentsQuery = appointmentsQuery.Where(a => a.ScheduledStart <= request.ToDate);

        // Revenue is attributed to a doctor via their completed appointments' invoices.
        // Grouped in SQL (via GroupBy translation) rather than pulled into memory first —
        // this stays reasonably fast even with a large appointment history.
        var rows = await appointmentsQuery
            .GroupBy(a => new { a.DoctorId, a.Doctor.FullName, a.Doctor.Specialization })
            .Select(g => new DoctorPerformanceReportRowDto(
                g.Key.FullName,
                g.Key.Specialization,
                g.Count(),
                g.Count(a => a.Status == AppointmentStatus.Completed),
                g.Count(a => a.Status == AppointmentStatus.Cancelled),
                g.Count(a => a.Status == AppointmentStatus.NoShow),
                g.Where(a => a.Invoice != null).Sum(a => a.Invoice!.Payments.Sum(p => p.Amount))))
            .OrderByDescending(r => r.TotalRevenue)
            .ToListAsync(cancellationToken);

        return rows;
    }
}
