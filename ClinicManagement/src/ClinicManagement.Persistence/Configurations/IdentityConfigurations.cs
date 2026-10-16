using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Persistence.Configurations;

public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.Property(u => u.FullName).IsRequired().HasMaxLength(200);

        // SuperAdmin users have TenantId == Guid.Empty and are excluded from the tenant filter
        // via ICurrentUserService.IsSuperAdmin. TenantId is a non-nullable Guid, so EF Core
        // enforces the FK column as NOT NULL no matter what IsRequired(false) says — there is no
        // way to make this FK "optional" while keeping a real foreign key constraint. Rather than
        // seed a dummy Tenant row with Id == Guid.Empty just to satisfy the constraint, we drop
        // the FK constraint entirely: TenantId stays a plain, indexed column used for filtering,
        // not a referential-integrity-checked foreign key. Regular tenant users are still
        // guaranteed valid by construction (RegisterTenantCommand/RegisterUserCommand always set
        // TenantId from an existing tenant), so this only relaxes the one SuperAdmin edge case.
        builder.Ignore(u => u.Tenant);
        builder.Property(u => u.TenantId).IsRequired();

        builder.HasOne(u => u.DoctorProfile)
            .WithOne(d => d.ApplicationUser)
            .HasForeignKey<Doctor>(d => d.ApplicationUserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(u => u.EmployeeProfile)
            .WithOne(e => e.ApplicationUser)
            .HasForeignKey<Employee>(e => e.ApplicationUserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(u => u.PatientProfile)
            .WithOne(p => p.ApplicationUser)
            .HasForeignKey<Patient>(p => p.ApplicationUserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(u => new { u.TenantId, u.NormalizedEmail });
    }
}

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.Property(rt => rt.Token).IsRequired().HasMaxLength(512);

        builder.HasOne(rt => rt.User)
            .WithMany(u => u.RefreshTokens)
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(rt => rt.Token).IsUnique();
    }
}
