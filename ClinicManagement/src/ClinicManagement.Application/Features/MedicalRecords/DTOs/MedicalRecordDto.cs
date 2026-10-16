using AutoMapper;
using ClinicManagement.Application.Common.Mappings;
using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Application.Features.MedicalRecords.DTOs;

public class MedicalRecordDto : IMapFrom<MedicalRecord>
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public string PatientName { get; set; } = default!;
    public Guid DoctorId { get; set; }
    public string DoctorName { get; set; } = default!;
    public Guid? AppointmentId { get; set; }
    public string Diagnosis { get; set; } = default!;
    public string? Symptoms { get; set; }
    public string? TreatmentPlan { get; set; }
    public string? Notes { get; set; }
    public DateTime? FollowUpDate { get; set; }
    public Guid? TemplateId { get; set; }
    public string? TemplateName { get; set; }
    public string? CustomFieldsJson { get; set; }
    public DateTime CreatedAt { get; set; }

    public void Mapping(Profile profile) =>
        profile.CreateMap<MedicalRecord, MedicalRecordDto>()
            .ForMember(d => d.PatientName, opt => opt.MapFrom(s => s.Patient.FullName))
            .ForMember(d => d.DoctorName, opt => opt.MapFrom(s => s.Doctor.FullName))
            .ForMember(d => d.TemplateName, opt => opt.MapFrom(s => s.Template != null ? s.Template.Name : null));
}
