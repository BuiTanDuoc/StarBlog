using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Persistence.Configurations;

public class MedicalRecordConfiguration : IEntityTypeConfiguration<MedicalRecord>
{
    public void Configure(EntityTypeBuilder<MedicalRecord> builder)
    {
        builder.Property(m => m.Diagnosis).IsRequired().HasMaxLength(1000);
        builder.Property(m => m.Symptoms).HasMaxLength(1000);
        builder.Property(m => m.TreatmentPlan).HasColumnType("nvarchar(max)");
        builder.Property(m => m.Notes).HasColumnType("nvarchar(max)");
        builder.Property(m => m.CustomFieldsJson).HasColumnType("nvarchar(max)");

        builder.HasOne(m => m.Patient)
            .WithMany(p => p.MedicalRecords)
            .HasForeignKey(m => m.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(m => m.Doctor)
            .WithMany(d => d.MedicalRecords)
            .HasForeignKey(m => m.DoctorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(m => m.Appointment)
            .WithOne(a => a.MedicalRecord)
            .HasForeignKey<MedicalRecord>(m => m.AppointmentId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(m => m.Template)
            .WithMany(t => t.MedicalRecords)
            .HasForeignKey(m => m.TemplateId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

public class MedicalRecordTemplateConfiguration : IEntityTypeConfiguration<MedicalRecordTemplate>
{
    public void Configure(EntityTypeBuilder<MedicalRecordTemplate> builder)
    {
        builder.Property(t => t.Name).IsRequired().HasMaxLength(150);
        builder.Property(t => t.Specialization).IsRequired().HasMaxLength(150);
        builder.Property(t => t.FieldsSchemaJson).IsRequired().HasColumnType("nvarchar(max)");

        builder.HasIndex(t => new { t.TenantId, t.Specialization });
    }
}

public class PrescriptionConfiguration : IEntityTypeConfiguration<Prescription>
{
    public void Configure(EntityTypeBuilder<Prescription> builder)
    {
        builder.HasOne(p => p.MedicalRecord)
            .WithMany(m => m.Prescriptions)
            .HasForeignKey(p => p.MedicalRecordId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(p => p.Patient)
            .WithMany()
            .HasForeignKey(p => p.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Doctor)
            .WithMany()
            .HasForeignKey(p => p.DoctorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class PrescriptionItemConfiguration : IEntityTypeConfiguration<PrescriptionItem>
{
    public void Configure(EntityTypeBuilder<PrescriptionItem> builder)
    {
        builder.Property(i => i.MedicineName).IsRequired().HasMaxLength(200);
        builder.Property(i => i.Dosage).IsRequired().HasMaxLength(100);
        builder.Property(i => i.Frequency).IsRequired().HasMaxLength(100);
        builder.Property(i => i.Duration).IsRequired().HasMaxLength(100);
        builder.Property(i => i.Instructions).HasMaxLength(500);

        builder.HasOne(i => i.Prescription)
            .WithMany(p => p.Items)
            .HasForeignKey(i => i.PrescriptionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
