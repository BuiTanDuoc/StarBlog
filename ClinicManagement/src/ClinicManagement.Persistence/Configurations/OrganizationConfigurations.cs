using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Persistence.Configurations;

public class BranchConfiguration : IEntityTypeConfiguration<Branch>
{
    public void Configure(EntityTypeBuilder<Branch> builder)
    {
        builder.Property(b => b.Name).IsRequired().HasMaxLength(200);
        builder.Property(b => b.Address).IsRequired().HasMaxLength(500);
        builder.Property(b => b.Phone).IsRequired().HasMaxLength(30);

        builder.HasIndex(b => new { b.TenantId, b.Name });
    }
}

public class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> builder)
    {
        builder.Property(d => d.Name).IsRequired().HasMaxLength(150);

        builder.HasOne(d => d.Branch)
            .WithMany(b => b.Departments)
            .HasForeignKey(d => d.BranchId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class DoctorConfiguration : IEntityTypeConfiguration<Doctor>
{
    public void Configure(EntityTypeBuilder<Doctor> builder)
    {
        builder.Property(d => d.FullName).IsRequired().HasMaxLength(200);
        builder.Property(d => d.Specialization).IsRequired().HasMaxLength(150);
        builder.Property(d => d.LicenseNumber).IsRequired().HasMaxLength(100);
        builder.Property(d => d.ConsultationFee).HasPrecision(18, 2);
        builder.Property(d => d.WorkScheduleJson).HasColumnType("nvarchar(max)");

        builder.HasOne(d => d.Branch)
            .WithMany(b => b.Doctors)
            .HasForeignKey(d => d.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(d => d.Department)
            .WithMany(dep => dep.Doctors)
            .HasForeignKey(d => d.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(d => new { d.TenantId, d.LicenseNumber }).IsUnique();
    }
}

public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.Property(e => e.FullName).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Position).IsRequired().HasMaxLength(100);
        builder.Property(e => e.Salary).HasPrecision(18, 2);

        builder.HasOne(e => e.Branch)
            .WithMany(b => b.Employees)
            .HasForeignKey(e => e.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Department)
            .WithMany()
            .HasForeignKey(e => e.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
