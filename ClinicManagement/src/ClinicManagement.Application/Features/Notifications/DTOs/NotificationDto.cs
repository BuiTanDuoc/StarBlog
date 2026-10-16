using ClinicManagement.Application.Common.Mappings;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;

namespace ClinicManagement.Application.Features.Notifications.DTOs;

public class NotificationDto : IMapFrom<Notification>
{
    public Guid Id { get; set; }
    public string Title { get; set; } = default!;
    public string Message { get; set; } = default!;
    public NotificationType Type { get; set; }
    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }
    public string? RelatedEntityId { get; set; }
    public DateTime CreatedAt { get; set; }
}
