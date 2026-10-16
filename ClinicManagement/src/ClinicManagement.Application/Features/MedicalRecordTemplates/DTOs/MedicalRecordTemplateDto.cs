using AutoMapper;
using ClinicManagement.Application.Common.Mappings;
using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Application.Features.MedicalRecordTemplates.DTOs;

public class MedicalRecordTemplateDto : IMapFrom<MedicalRecordTemplate>
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string Specialization { get; set; } = default!;
    public bool IsActive { get; set; }
    public string FieldsSchemaJson { get; set; } = "[]";

    public void Mapping(Profile profile) =>
        profile.CreateMap<MedicalRecordTemplate, MedicalRecordTemplateDto>();
}
