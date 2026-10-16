using AutoMapper;
using ClinicManagement.Application.Common.Mappings;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;

namespace ClinicManagement.Application.Features.Appointments.DTOs;

public class AppointmentDto : IMapFrom<Appointment>
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public string PatientName { get; set; } = default!;
    public Guid DoctorId { get; set; }
    public string DoctorName { get; set; } = default!;
    public Guid BranchId { get; set; }
    public string BranchName { get; set; } = default!;
    public DateTime ScheduledStart { get; set; }
    public DateTime ScheduledEnd { get; set; }
    public AppointmentStatus Status { get; set; }
    public string? Notes { get; set; }
    public string? CancellationReason { get; set; }
    public Guid? PatientPackageId { get; set; }
    public DateTime CreatedAt { get; set; }

    public void Mapping(Profile profile) =>
        profile.CreateMap<Appointment, AppointmentDto>()
            .ForMember(d => d.PatientName, opt => opt.MapFrom(s => s.Patient.FullName))
            .ForMember(d => d.DoctorName, opt => opt.MapFrom(s => s.Doctor.FullName))
            .ForMember(d => d.BranchName, opt => opt.MapFrom(s => s.Branch.Name));
}

public class AppointmentStatusHistoryDto : IMapFrom<AppointmentStatusHistory>
{
    public AppointmentStatus FromStatus { get; set; }
    public AppointmentStatus ToStatus { get; set; }
    public DateTime ChangedAt { get; set; }
    public string? ChangedBy { get; set; }
    public string? Reason { get; set; }
}
