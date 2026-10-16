using ClinicManagement.Application.Common.Mappings;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;

namespace ClinicManagement.Application.Features.Plans.DTOs;

public class PlanDto : IMapFrom<Plan>
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public PlanTier Tier { get; set; }
    public decimal Price { get; set; }
    public int DurationInDays { get; set; }
    public int MaxDoctors { get; set; }
    public int MaxEmployees { get; set; }
    public int MaxPatients { get; set; }
    public int MaxAppointmentsPerMonth { get; set; }
    public long StorageLimitMb { get; set; }
    public bool IsActive { get; set; }
}
