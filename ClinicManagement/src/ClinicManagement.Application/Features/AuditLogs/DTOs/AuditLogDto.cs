using ClinicManagement.Application.Common.Mappings;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;

namespace ClinicManagement.Application.Features.AuditLogs.DTOs;

public class AuditLogDto : IMapFrom<AuditLog>
{
    public Guid Id { get; set; }
    public Guid? UserId { get; set; }
    public AuditAction Action { get; set; }
    public string EntityName { get; set; } = default!;
    public string EntityId { get; set; } = default!;
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public DateTime Timestamp { get; set; }
    public string? IpAddress { get; set; }
}
