using ClinicManagement.Application.Common.Mappings;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;

namespace ClinicManagement.Application.Features.Subscriptions.DTOs;

public class SubscriptionDto : IMapFrom<Subscription>
{
    public Guid Id { get; set; }
    public Guid PlanId { get; set; }
    public string PlanName { get; set; } = default!;
    public PlanTier PlanTier { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public SubscriptionStatus Status { get; set; }
    public bool AutoRenew { get; set; }

    public void Mapping(AutoMapper.Profile profile) =>
        profile.CreateMap<Subscription, SubscriptionDto>()
            .ForMember(d => d.PlanName, opt => opt.MapFrom(s => s.Plan.Name))
            .ForMember(d => d.PlanTier, opt => opt.MapFrom(s => s.Plan.Tier));
}

/// <summary>Current subscription plus live usage counts — powers the "3 of 10 doctors used" style UI on the ClinicOwner dashboard.</summary>
public record UsageStatDto(int Used, int Max)
{
    public bool IsUnlimited => Max == int.MaxValue;
    public double PercentUsed => IsUnlimited || Max == 0 ? 0 : Math.Round(Used * 100.0 / Max, 1);
}

public record SubscriptionWithUsageDto(
    SubscriptionDto Subscription,
    UsageStatDto Doctors,
    UsageStatDto Employees,
    UsageStatDto Patients,
    UsageStatDto AppointmentsThisMonth);
