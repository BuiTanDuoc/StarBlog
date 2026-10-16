using AutoMapper;
using ClinicManagement.Application.Common.Mappings;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;

namespace ClinicManagement.Application.Features.Patients.DTOs;

/// <summary>Full detail shape, used by GetPatientById.</summary>
public class PatientDto : IMapFrom<Patient>
{
    public Guid Id { get; set; }
    public string PatientCode { get; set; } = default!;
    public string? NationalId { get; set; }
    public string FullName { get; set; } = default!;
    public Gender Gender { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string Phone { get; set; } = default!;
    public string? Email { get; set; }
    public string? Address { get; set; }
    public BloodGroup BloodGroup { get; set; }
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }
    public string? Allergies { get; set; }
    public string? MedicalHistory { get; set; }
    public string? InsuranceProvider { get; set; }
    public string? InsuranceNumber { get; set; }
    public DateTime CreatedAt { get; set; }

    public int Age => DateTime.UtcNow.Year - DateOfBirth.Year - (DateTime.UtcNow.DayOfYear < DateOfBirth.DayOfYear ? 1 : 0);
}

/// <summary>Slim shape for list/grid views — avoids shipping medical history over the wire for every row.</summary>
public class PatientListItemDto : IMapFrom<Patient>
{
    public Guid Id { get; set; }
    public string PatientCode { get; set; } = default!;
    public string FullName { get; set; } = default!;
    public Gender Gender { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string Phone { get; set; } = default!;
    public DateTime CreatedAt { get; set; }

    public void Mapping(Profile profile) =>
        profile.CreateMap<Patient, PatientListItemDto>();
}
