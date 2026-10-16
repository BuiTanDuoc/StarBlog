using ClinicManagement.Application.Common.Mappings;
using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Application.Features.Branches.DTOs;

public class BranchDto : IMapFrom<Branch>
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string Address { get; set; } = default!;
    public string Phone { get; set; } = default!;
    public string? Email { get; set; }
    public bool IsActive { get; set; }
}
