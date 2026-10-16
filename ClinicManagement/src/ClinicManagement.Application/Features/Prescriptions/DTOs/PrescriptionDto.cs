using AutoMapper;
using ClinicManagement.Application.Common.Mappings;
using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Application.Features.Prescriptions.DTOs;

public class PrescriptionItemDto : IMapFrom<PrescriptionItem>
{
    public string MedicineName { get; set; } = default!;
    public string Dosage { get; set; } = default!;
    public string Frequency { get; set; } = default!;
    public string Duration { get; set; } = default!;
    public string? Instructions { get; set; }
}

public class PrescriptionDto : IMapFrom<Prescription>
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public string PatientName { get; set; } = default!;
    public Guid DoctorId { get; set; }
    public string DoctorName { get; set; } = default!;
    public DateTime DateIssued { get; set; }
    public string? GeneratedPdfUrl { get; set; }
    public List<PrescriptionItemDto> Items { get; set; } = new();

    /// <summary>
    /// Populated only by CreatePrescriptionCommand (via IDrugSafetyChecker), not stored on
    /// the entity or returned by GetPrescriptionById — these are point-in-time warnings for
    /// the prescribing doctor to see immediately, not a persisted audit trail.
    /// </summary>
    public List<string> Warnings { get; set; } = new();

    public void Mapping(Profile profile) =>
        profile.CreateMap<Prescription, PrescriptionDto>()
            .ForMember(d => d.PatientName, opt => opt.MapFrom(s => s.Patient.FullName))
            .ForMember(d => d.DoctorName, opt => opt.MapFrom(s => s.Doctor.FullName))
            .ForMember(d => d.Warnings, opt => opt.Ignore());
}
