using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Persistence.Configurations;

public class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
{
    public void Configure(EntityTypeBuilder<Appointment> builder)
    {
        builder.Property(a => a.Notes).HasMaxLength(2000);
        builder.Property(a => a.CancellationReason).HasMaxLength(500);

        builder.HasOne(a => a.Patient)
            .WithMany(p => p.Appointments)
            .HasForeignKey(a => a.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.Doctor)
            .WithMany(d => d.Appointments)
            .HasForeignKey(a => a.DoctorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.Branch)
            .WithMany(b => b.Appointments)
            .HasForeignKey(a => a.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.Invoice)
            .WithOne(i => i.Appointment)
            .HasForeignKey<Invoice>(i => i.AppointmentId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(a => a.PatientPackage)
            .WithMany(p => p.Appointments)
            .HasForeignKey(a => a.PatientPackageId)
            .OnDelete(DeleteBehavior.SetNull);

        // Backs the doctor-availability / conflict-detection query:
        // WHERE DoctorId = @doctorId AND ScheduledStart < @end AND ScheduledEnd > @start
        builder.HasIndex(a => new { a.DoctorId, a.ScheduledStart, a.ScheduledEnd });
        builder.HasIndex(a => new { a.TenantId, a.Status });
    }
}

public class AppointmentStatusHistoryConfiguration : IEntityTypeConfiguration<AppointmentStatusHistory>
{
    public void Configure(EntityTypeBuilder<AppointmentStatusHistory> builder)
    {
        builder.Property(h => h.Reason).HasMaxLength(500);

        builder.HasOne(h => h.Appointment)
            .WithMany(a => a.StatusHistory)
            .HasForeignKey(h => h.AppointmentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
