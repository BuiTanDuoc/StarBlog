using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Persistence.Configurations;

public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.Property(i => i.InvoiceNumber).IsRequired().HasMaxLength(50);
        builder.Property(i => i.SubTotal).HasPrecision(18, 2);
        builder.Property(i => i.Discount).HasPrecision(18, 2);
        builder.Property(i => i.Tax).HasPrecision(18, 2);
        builder.Property(i => i.Total).HasPrecision(18, 2);

        // Computed properties (AmountPaid, Balance) aren't mapped — EF Core skips
        // get-only properties with no backing field by convention, but we're explicit here.
        builder.Ignore(i => i.AmountPaid);
        builder.Ignore(i => i.Balance);

        builder.HasOne(i => i.Patient)
            .WithMany(p => p.Invoices)
            .HasForeignKey(i => i.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(i => new { i.TenantId, i.InvoiceNumber }).IsUnique();
    }
}

public class InvoiceItemConfiguration : IEntityTypeConfiguration<InvoiceItem>
{
    public void Configure(EntityTypeBuilder<InvoiceItem> builder)
    {
        builder.Property(i => i.Description).IsRequired().HasMaxLength(500);
        builder.Property(i => i.UnitPrice).HasPrecision(18, 2);
        builder.Property(i => i.Total).HasPrecision(18, 2);

        builder.HasOne(i => i.Invoice)
            .WithMany(inv => inv.Items)
            .HasForeignKey(i => i.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.Property(p => p.Amount).HasPrecision(18, 2);

        builder.HasOne(p => p.Invoice)
            .WithMany(i => i.Payments)
            .HasForeignKey(p => p.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class ExpenseConfiguration : IEntityTypeConfiguration<Expense>
{
    public void Configure(EntityTypeBuilder<Expense> builder)
    {
        builder.Property(e => e.Category).IsRequired().HasMaxLength(100);
        builder.Property(e => e.Amount).HasPrecision(18, 2);

        builder.HasOne(e => e.Branch)
            .WithMany()
            .HasForeignKey(e => e.BranchId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class TreatmentPackageConfiguration : IEntityTypeConfiguration<TreatmentPackage>
{
    public void Configure(EntityTypeBuilder<TreatmentPackage> builder)
    {
        builder.Property(t => t.Name).IsRequired().HasMaxLength(150);
        builder.Property(t => t.Description).HasMaxLength(1000);
        builder.Property(t => t.Price).HasPrecision(18, 2);
    }
}

public class PatientPackageConfiguration : IEntityTypeConfiguration<PatientPackage>
{
    public void Configure(EntityTypeBuilder<PatientPackage> builder)
    {
        builder.Ignore(p => p.SessionsRemaining);
        builder.Ignore(p => p.IsExpired);

        builder.HasOne(p => p.Patient)
            .WithMany()
            .HasForeignKey(p => p.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.TreatmentPackage)
            .WithMany(t => t.PatientPackages)
            .HasForeignKey(p => p.TreatmentPackageId)
            .OnDelete(DeleteBehavior.Restrict);

        // Restrict, not Cascade/SetNull: an invoice for a sold package should never be
        // deleted out from under the PatientPackage that accounts for it — same reasoning
        // as Invoice.PatientId elsewhere in this file.
        builder.HasOne(p => p.Invoice)
            .WithMany()
            .HasForeignKey(p => p.InvoiceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(p => new { p.TenantId, p.PatientId });
    }
}
