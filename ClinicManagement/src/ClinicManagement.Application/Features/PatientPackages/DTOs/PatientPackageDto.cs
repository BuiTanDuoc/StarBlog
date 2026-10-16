using AutoMapper;
using ClinicManagement.Application.Common.Mappings;
using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Application.Features.PatientPackages.DTOs;

public class PatientPackageDto : IMapFrom<PatientPackage>
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public Guid TreatmentPackageId { get; set; }
    public string TreatmentPackageName { get; set; } = default!;
    public Guid? InvoiceId { get; set; }
    public int SessionsTotal { get; set; }
    public int SessionsUsed { get; set; }
    public int SessionsRemaining { get; set; }
    public DateTime PurchasedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsExpired { get; set; }

    public void Mapping(Profile profile) =>
        profile.CreateMap<PatientPackage, PatientPackageDto>()
            .ForMember(d => d.TreatmentPackageName, opt => opt.MapFrom(s => s.TreatmentPackage.Name))
            .ForMember(d => d.SessionsRemaining, opt => opt.MapFrom(s => s.SessionsTotal - s.SessionsUsed))
            .ForMember(d => d.IsExpired, opt => opt.MapFrom(s => DateTime.UtcNow > s.ExpiresAt));
}
