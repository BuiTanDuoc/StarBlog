using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Features.Reports.DTOs;
using ClinicManagement.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Application.Features.Reports.Queries.GetRevenueReport;

public record GetRevenueReportQuery(DateTime? FromDate, DateTime? ToDate) : IRequest<List<RevenueReportRowDto>>;

public class GetRevenueReportQueryHandler : IRequestHandler<GetRevenueReportQuery, List<RevenueReportRowDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetRevenueReportQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<RevenueReportRowDto>> Handle(GetRevenueReportQuery request, CancellationToken cancellationToken)
    {
        var query = _unitOfWork.Repository<Payment>().Query();

        if (request.FromDate.HasValue) query = query.Where(p => p.PaidAt >= request.FromDate);
        if (request.ToDate.HasValue) query = query.Where(p => p.PaidAt <= request.ToDate);

        return await query
            .OrderByDescending(p => p.PaidAt)
            .Select(p => new RevenueReportRowDto(
                p.PaidAt,
                p.Invoice.Patient.FullName,
                p.Invoice.InvoiceNumber,
                p.Amount,
                p.Method.ToString()))
            .ToListAsync(cancellationToken);
    }
}
