using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Features.Subscriptions.DTOs;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Application.Features.Subscriptions.Queries.GetCurrentSubscription;

public record GetCurrentSubscriptionQuery : IRequest<Result<SubscriptionWithUsageDto>>;

public class GetCurrentSubscriptionQueryHandler
    : IRequestHandler<GetCurrentSubscriptionQuery, Result<SubscriptionWithUsageDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly AutoMapper.IMapper _mapper;

    public GetCurrentSubscriptionQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService, AutoMapper.IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _mapper = mapper;
    }

    public async Task<Result<SubscriptionWithUsageDto>> Handle(
        GetCurrentSubscriptionQuery request, CancellationToken cancellationToken)
    {
        var subscription = await _unitOfWork.Repository<Subscription>().Query()
            .Include(s => s.Plan)
            .Where(s => s.TenantId == _currentUserService.TenantId
                        && (s.Status == SubscriptionStatus.Active || s.Status == SubscriptionStatus.Trialing))
            .OrderByDescending(s => s.EndDate)
            .FirstOrDefaultAsync(cancellationToken);

        if (subscription is null)
        {
            return Result<SubscriptionWithUsageDto>.Failure("No active subscription found for this clinic.");
        }

        var plan = subscription.Plan;

        var doctorCount = await _unitOfWork.Repository<Doctor>().Query().CountAsync(cancellationToken);
        var employeeCount = await _unitOfWork.Repository<Employee>().Query().CountAsync(cancellationToken);
        var patientCount = await _unitOfWork.Repository<Patient>().Query().CountAsync(cancellationToken);

        var monthStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var appointmentsThisMonth = await _unitOfWork.Repository<Appointment>().Query()
            .CountAsync(a => a.CreatedAt >= monthStart, cancellationToken);

        var dto = new SubscriptionWithUsageDto(
            _mapper.Map<SubscriptionDto>(subscription),
            new UsageStatDto(doctorCount, plan.MaxDoctors),
            new UsageStatDto(employeeCount, plan.MaxEmployees),
            new UsageStatDto(patientCount, plan.MaxPatients),
            new UsageStatDto(appointmentsThisMonth, plan.MaxAppointmentsPerMonth));

        return Result<SubscriptionWithUsageDto>.Success(dto);
    }
}
