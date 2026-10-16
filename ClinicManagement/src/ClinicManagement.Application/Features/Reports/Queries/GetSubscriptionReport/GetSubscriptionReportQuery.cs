using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Features.Reports.DTOs;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Application.Features.Reports.Queries.GetSubscriptionReport;

public record GetSubscriptionReportQuery : IRequest<Result<List<SubscriptionReportRowDto>>>;

public class GetSubscriptionReportQueryHandler
    : IRequestHandler<GetSubscriptionReportQuery, Result<List<SubscriptionReportRowDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public GetSubscriptionReportQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<List<SubscriptionReportRowDto>>> Handle(
        GetSubscriptionReportQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsSuperAdmin)
        {
            return Result<List<SubscriptionReportRowDto>>.Failure("Only platform administrators can view the cross-tenant subscription report.");
        }

        // IgnoreQueryFilters: deliberately cross-tenant — the one report where that's correct
        // instead of a bug (contrast with every other query in this codebase, and see the
        // identical pattern/comment in ExpiredSubscriptionsJob).
        var rows = await _unitOfWork.Repository<Subscription>().Query()
            .Include(t=>t.Tenant)
            .IgnoreQueryFilters()
            .Where(s => !s.IsDeleted)
            .OrderByDescending(s => s.EndDate)
            .Select(s => new SubscriptionReportRowDto(
                s.Tenant.Name,
                s.Plan.Name,
                s.StartDate,
                s.EndDate,
                s.Status.ToString(),
                s.Payments.Sum(p => p.Amount)))
            .ToListAsync(cancellationToken);

        return Result<List<SubscriptionReportRowDto>>.Success(rows);
    }
}
