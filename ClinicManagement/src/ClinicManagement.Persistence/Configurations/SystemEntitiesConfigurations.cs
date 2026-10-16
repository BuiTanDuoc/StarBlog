using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicManagement.Persistence.Configurations;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.Property(n => n.Title).IsRequired().HasMaxLength(200);
        builder.Property(n => n.Message).IsRequired().HasMaxLength(1000);

        builder.HasOne(n => n.User)
            .WithMany()
            .HasForeignKey(n => n.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(n => new { n.UserId, n.IsRead });
    }
}

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.Property(a => a.EntityName).IsRequired().HasMaxLength(200);
        builder.Property(a => a.EntityId).IsRequired().HasMaxLength(64);
        builder.Property(a => a.OldValues).HasColumnType("nvarchar(max)");
        builder.Property(a => a.NewValues).HasColumnType("nvarchar(max)");
        builder.Property(a => a.IpAddress).HasMaxLength(64);

        builder.HasIndex(a => new { a.TenantId, a.EntityName, a.EntityId });
        builder.HasIndex(a => a.Timestamp);
    }
}

public class FileAttachmentConfiguration : IEntityTypeConfiguration<FileAttachment>
{
    public void Configure(EntityTypeBuilder<FileAttachment> builder)
    {
        builder.Property(f => f.FileName).IsRequired().HasMaxLength(300);
        builder.Property(f => f.StoragePath).IsRequired().HasMaxLength(1000);
        builder.Property(f => f.ContentType).IsRequired().HasMaxLength(150);
        builder.Property(f => f.EntityName).IsRequired().HasMaxLength(100);

        builder.HasOne(f => f.MedicalRecord)
            .WithMany(m => m.Attachments)
            .HasForeignKey(f => f.MedicalRecordId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(f => new { f.EntityName, f.EntityId });
    }
}
