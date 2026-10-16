using AutoMapper;
using ClinicManagement.Application.Common.Mappings;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;

namespace ClinicManagement.Application.Features.Doctors.DTOs;

public class DoctorDto : IMapFrom<Doctor>
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = default!;
    public string Specialization { get; set; } = default!;
    public string LicenseNumber { get; set; } = default!;
    public decimal ConsultationFee { get; set; }
    public string WorkScheduleJson { get; set; } = default!;
    public int YearsOfExperience { get; set; }
    public DoctorStatus Status { get; set; }
    public Guid BranchId { get; set; }
    public string BranchName { get; set; } = default!;
    public Guid? DepartmentId { get; set; }
    public Guid? ApplicationUserId { get; set; }

    public void Mapping(Profile profile) =>
        profile.CreateMap<Doctor, DoctorDto>()
            .ForMember(d => d.BranchName, opt => opt.MapFrom(s => s.Branch.Name));
}

public class DoctorListItemDto : IMapFrom<Doctor>
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = default!;
    public string Specialization { get; set; } = default!;
    public DoctorStatus Status { get; set; }
    public decimal ConsultationFee { get; set; }

    /// <summary>Needed by booking flows (staff and patient-portal alike) since CreateAppointmentCommand requires a BranchId — not just cosmetic.</summary>
    public Guid BranchId { get; set; }
}
