using ClinicManagement.API.Hubs;
using ClinicManagement.Application.Common.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace ClinicManagement.API.Services;

public class SignalRNotificationDispatcher : INotificationDispatcher
{
    private readonly IHubContext<NotificationHub> _hubContext;

    public SignalRNotificationDispatcher(IHubContext<NotificationHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public Task SendToUserAsync(Guid userId, string title, string message, string type, CancellationToken cancellationToken = default) =>
        _hubContext.Clients.Group($"user:{userId}")
            .SendAsync("ReceiveNotification", new { title, message, type, timestamp = DateTime.UtcNow }, cancellationToken);

    public Task SendToTenantAsync(Guid tenantId, string title, string message, string type, CancellationToken cancellationToken = default) =>
        _hubContext.Clients.Group($"tenant:{tenantId}")
            .SendAsync("ReceiveNotification", new { title, message, type, timestamp = DateTime.UtcNow }, cancellationToken);
}
