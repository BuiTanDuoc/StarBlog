using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Features.Dashboard.DTOs;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Application.Features.Dashboard.Queries.GetClinicOwnerDashboard;

public record GetClinicOwnerDashboardQuery : IRequest<ClinicOwnerDashboardDto>;

public class GetClinicOwnerDashboardQueryHandler : IRequestHandler<GetClinicOwnerDashboardQuery, ClinicOwnerDashboardDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public GetClinicOwnerDashboardQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<ClinicOwnerDashboardDto> Handle(GetClinicOwnerDashboardQuery request, CancellationToken cancellationToken)
    {
        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);
        var monthStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var totalPatients = await _unitOfWork.Repository<Patient>().Query().CountAsync(cancellationToken);
        var totalDoctors = await _unitOfWork.Repository<Doctor>().Query().CountAsync(cancellationToken);

        var appointmentsToday = await _unitOfWork.Repository<Appointment>().Query()
            .CountAsync(a => a.ScheduledStart >= today && a.ScheduledStart < tomorrow, cancellationToken);

        // Revenue = payments actually received this month, not invoice totals issued — matches
        // the Revenue Report's definition (Features/Reports/Queries/GetRevenueReport) for consistency.
        var revenueThisMonth = await _unitOfWork.Repository<Payment>().Query()
            .Where(p => p.PaidAt >= monthStart)
            .SumAsync(p => (decimal?)p.Amount, cancellationToken) ?? 0m;

        var subscription = await _unitOfWork.Repository<Subscription>().Query()
            .Include(s => s.Plan)
            .Where(s => s.TenantId == _currentUserService.TenantId
                        && (s.Status == SubscriptionStatus.Active || s.Status == SubscriptionStatus.Trialing))
            .OrderByDescending(s => s.EndDate)
            .FirstOrDefaultAsync(cancellationToken);

        var subscriptionSummary = subscription is null
            ? null
            : new SubscriptionSummaryDto(
                subscription.Plan.Name,
                subscription.EndDate,
                subscription.Status.ToString(),
                Math.Max(0, (subscription.EndDate.Date - today).Days));

        return new ClinicOwnerDashboardDto(totalPatients, totalDoctors, appointmentsToday, revenueThisMonth, subscriptionSummary);
    }
}
