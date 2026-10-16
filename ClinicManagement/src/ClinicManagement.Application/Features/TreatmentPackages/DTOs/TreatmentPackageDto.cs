using AutoMapper;
using ClinicManagement.Application.Common.Mappings;
using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Application.Features.TreatmentPackages.DTOs;

public class TreatmentPackageDto : IMapFrom<TreatmentPackage>
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public int SessionsIncluded { get; set; }
    public decimal Price { get; set; }
    public int ValidityDays { get; set; }
    public bool IsActive { get; set; }

    public void Mapping(Profile profile) => profile.CreateMap<TreatmentPackage, TreatmentPackageDto>();
}
