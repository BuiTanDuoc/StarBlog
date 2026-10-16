using AutoMapper;
using ClinicManagement.Application.Common.Mappings;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;

namespace ClinicManagement.Application.Features.Employees.DTOs;

public class EmployeeDto : IMapFrom<Employee>
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = default!;
    public string Position { get; set; } = default!;
    public DateTime HireDate { get; set; }
    public decimal Salary { get; set; }
    public EmployeeStatus Status { get; set; }
    public Guid BranchId { get; set; }
    public string BranchName { get; set; } = default!;
    public Guid? DepartmentId { get; set; }
    public Guid? ApplicationUserId { get; set; }

    public void Mapping(Profile profile) =>
        profile.CreateMap<Employee, EmployeeDto>()
            .ForMember(d => d.BranchName, opt => opt.MapFrom(s => s.Branch.Name));
}

public class EmployeeListItemDto : IMapFrom<Employee>
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = default!;
    public string Position { get; set; } = default!;
    public EmployeeStatus Status { get; set; }
}
