using ClinicManagement.Application.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace ClinicManagement.API.Hubs;

/// <summary>
/// Every connection joins two groups on connect: "user:{userId}" (for
/// notifications targeted at exactly one person, e.g. "your appointment was
/// cancelled") and "tenant:{tenantId}" (for clinic-wide broadcasts, e.g. a
/// receptionist dashboard live-updating when any appointment changes).
/// Requires a valid JWT — SignalR reads the same bearer token via the
/// access_token query string param on the initial handshake (configured in
/// Program.cs) since browsers can't set an Authorization header on the
/// WebSocket upgrade request.
/// </summary>
[Authorize]
public class NotificationHub : Hub
{
    private readonly ICurrentUserService _currentUserService;

    public NotificationHub(ICurrentUserService currentUserService)
    {
        _currentUserService = currentUserService;
    }

    public override async Task OnConnectedAsync()
    {
        if (_currentUserService.UserId is { } userId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user:{userId}");
        }

        if (_currentUserService.TenantId != Guid.Empty)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"tenant:{_currentUserService.TenantId}");
        }

        await base.OnConnectedAsync();
    }
}
