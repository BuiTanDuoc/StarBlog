using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Persistence.Configurations;

public class PatientConfiguration : IEntityTypeConfiguration<Patient>
{
    public void Configure(EntityTypeBuilder<Patient> builder)
    {
        builder.Property(p => p.PatientCode).IsRequired().HasMaxLength(30);
        builder.Property(p => p.FullName).IsRequired().HasMaxLength(200);
        builder.Property(p => p.Phone).IsRequired().HasMaxLength(30);
        builder.Property(p => p.NationalId).HasMaxLength(50);
        builder.Property(p => p.Address).HasMaxLength(500);
        builder.Property(p => p.Email).HasMaxLength(256);
        builder.Property(p => p.Allergies).HasMaxLength(1000);
        builder.Property(p => p.MedicalHistory).HasColumnType("nvarchar(max)");

        // Unique per tenant, not globally — two clinics may both assign "PT-000001".
        builder.HasIndex(p => new { p.TenantId, p.PatientCode }).IsUnique();
        builder.HasIndex(p => new { p.TenantId, p.NationalId });
        builder.HasIndex(p => new { p.TenantId, p.FullName });

        // ApplicationUserId already gets a unique-per-principal-end index from the 1:1
        // HasForeignKey<Patient> configuration in IdentityConfigurations — no extra index needed here.
    }
}
