using ClinicManagement.Application.Common.Mappings;
using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Application.Features.Departments.DTOs;

public class DepartmentDto : IMapFrom<Department>
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public Guid BranchId { get; set; }

    public void Mapping(AutoMapper.Profile profile) =>
        profile.CreateMap<Department, DepartmentDto>();
}
