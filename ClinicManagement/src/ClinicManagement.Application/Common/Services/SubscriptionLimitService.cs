using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;
using ClinicManagement.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Application.Common.Services;

/// <summary>
/// Checked at the start of every command that creates a plan-limited
/// resource (Patient, Doctor, Employee, Appointment). This is the
/// business-logic-level check the spec calls for ("verify subscription
/// limits before creating records"); SubscriptionGuardMiddleware (API
/// layer, Phase 5) is a coarser, faster-failing check at the HTTP level for
/// the case where the whole subscription itself has expired. Both exist —
/// the middleware stops expired tenants at the door, this stops active
/// tenants from exceeding their specific plan quotas.
/// </summary>
public interface ISubscriptionLimitService
{
    Task EnsureCanAddPatientAsync(CancellationToken cancellationToken = default);
    Task EnsureCanAddDoctorAsync(CancellationToken cancellationToken = default);
    Task EnsureCanAddEmployeeAsync(CancellationToken cancellationToken = default);
    Task EnsureCanCreateAppointmentThisMonthAsync(CancellationToken cancellationToken = default);
}

public class SubscriptionLimitService : ISubscriptionLimitService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public SubscriptionLimitService(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task EnsureCanAddPatientAsync(CancellationToken cancellationToken = default)
    {
        var plan = await GetActivePlanAsync(cancellationToken);
        var count = await _unitOfWork.Repository<Patient>().Query().CountAsync(cancellationToken);

        if (count >= plan.MaxPatients)
        {
            throw new SubscriptionLimitExceededException("Patients", plan.MaxPatients);
        }
    }

    public async Task EnsureCanAddDoctorAsync(CancellationToken cancellationToken = default)
    {
        var plan = await GetActivePlanAsync(cancellationToken);
        var count = await _unitOfWork.Repository<Doctor>().Query().CountAsync(cancellationToken);

        if (count >= plan.MaxDoctors)
        {
            throw new SubscriptionLimitExceededException("Doctors", plan.MaxDoctors);
        }
    }

    public async Task EnsureCanAddEmployeeAsync(CancellationToken cancellationToken = default)
    {
        var plan = await GetActivePlanAsync(cancellationToken);
        var count = await _unitOfWork.Repository<Employee>().Query().CountAsync(cancellationToken);

        if (count >= plan.MaxEmployees)
        {
            throw new SubscriptionLimitExceededException("Employees", plan.MaxEmployees);
        }
    }

    public async Task EnsureCanCreateAppointmentThisMonthAsync(CancellationToken cancellationToken = default)
    {
        var plan = await GetActivePlanAsync(cancellationToken);

        var monthStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var monthEnd = monthStart.AddMonths(1);

        var count = await _unitOfWork.Repository<Appointment>().Query()
            .CountAsync(a => a.CreatedAt >= monthStart && a.CreatedAt < monthEnd, cancellationToken);

        if (count >= plan.MaxAppointmentsPerMonth)
        {
            throw new SubscriptionLimitExceededException("Appointments per month", plan.MaxAppointmentsPerMonth);
        }
    }

    private async Task<Plan> GetActivePlanAsync(CancellationToken cancellationToken)
    {
        var subscription = await _unitOfWork.Repository<Subscription>().Query()
            .Include(s => s.Plan)
            .Where(s => s.TenantId == _currentUserService.TenantId
                        && (s.Status == SubscriptionStatus.Active || s.Status == SubscriptionStatus.Trialing))
            .OrderByDescending(s => s.EndDate)
            .FirstOrDefaultAsync(cancellationToken);

        if (subscription is null)
        {
            throw new DomainException("No active subscription found for this clinic. Please subscribe to a plan to continue.");
        }

        return subscription.Plan;
    }
}
