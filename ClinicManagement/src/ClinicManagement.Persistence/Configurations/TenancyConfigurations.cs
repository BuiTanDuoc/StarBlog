using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Persistence.Configurations;

public class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.Property(t => t.Name).IsRequired().HasMaxLength(200);
        builder.Property(t => t.Slug).IsRequired().HasMaxLength(100);
        builder.Property(t => t.ContactEmail).IsRequired().HasMaxLength(256);
        builder.Property(t => t.ContactPhone).HasMaxLength(30);

        builder.HasIndex(t => t.Slug).IsUnique();
        builder.HasIndex(t => t.ContactEmail).IsUnique();

        // See ApplicationUserConfiguration for why: ApplicationUser.TenantId is intentionally a
        // plain, unenforced column (not a real FK) so SuperAdmin's TenantId == Guid.Empty is legal.
        // Ignoring this navigation too prevents EF's convention-based relationship discovery from
        // silently re-establishing the exact same FK constraint from the Tenant side.
        builder.Ignore(t => t.Users);
    }
}

public class PlanConfiguration : IEntityTypeConfiguration<Plan>
{
    public void Configure(EntityTypeBuilder<Plan> builder)
    {
        builder.Property(p => p.Name).IsRequired().HasMaxLength(100);
        builder.Property(p => p.Price).HasPrecision(18, 2);
    }
}

public class SubscriptionConfiguration : IEntityTypeConfiguration<Subscription>
{
    public void Configure(EntityTypeBuilder<Subscription> builder)
    {
        builder.HasOne(s => s.Plan)
            .WithMany(p => p.Subscriptions)
            .HasForeignKey(s => s.PlanId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(s => new { s.TenantId, s.Status });
    }
}

public class SubscriptionPaymentConfiguration : IEntityTypeConfiguration<SubscriptionPayment>
{
    public void Configure(EntityTypeBuilder<SubscriptionPayment> builder)
    {
        builder.Property(p => p.Amount).HasPrecision(18, 2);

        builder.HasOne(p => p.Subscription)
            .WithMany(s => s.Payments)
            .HasForeignKey(p => p.SubscriptionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
