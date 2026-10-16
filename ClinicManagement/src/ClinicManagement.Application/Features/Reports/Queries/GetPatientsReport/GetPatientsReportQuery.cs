using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Features.Reports.DTOs;
using ClinicManagement.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Application.Features.Reports.Queries.GetPatientsReport;

public record GetPatientsReportQuery(DateTime? FromDate, DateTime? ToDate) : IRequest<List<PatientsReportRowDto>>;

public class GetPatientsReportQueryHandler : IRequestHandler<GetPatientsReportQuery, List<PatientsReportRowDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetPatientsReportQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<PatientsReportRowDto>> Handle(GetPatientsReportQuery request, CancellationToken cancellationToken)
    {
        var query = _unitOfWork.Repository<Patient>().Query();

        if (request.FromDate.HasValue) query = query.Where(p => p.CreatedAt >= request.FromDate);
        if (request.ToDate.HasValue) query = query.Where(p => p.CreatedAt <= request.ToDate);

        return await query
            .OrderBy(p => p.FullName)
            .Select(p => new PatientsReportRowDto(
                p.PatientCode,
                p.FullName,
                p.Gender.ToString(),
                p.DateOfBirth,
                p.Phone,
                p.CreatedAt,
                p.Appointments.Count))
            .ToListAsync(cancellationToken);
    }
}
