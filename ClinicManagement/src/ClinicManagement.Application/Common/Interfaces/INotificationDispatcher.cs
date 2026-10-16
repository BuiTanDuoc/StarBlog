namespace ClinicManagement.Application.Common.Interfaces;

/// <summary>
/// Pushes a real-time event over SignalR. Implemented in the API project
/// (ClinicManagement.API/Services/SignalRNotificationDispatcher.cs) because
/// only that layer knows about the Hub type — Application and Infrastructure
/// depend only on this interface, and the concrete SignalR implementation is
/// wired up in Program.cs. This is what lets a background job in
/// Infrastructure (ExpiredSubscriptionsJob) push live notifications without
/// Infrastructure ever referencing API.
/// </summary>
public interface INotificationDispatcher
{
    Task SendToUserAsync(Guid userId, string title, string message, string type, CancellationToken cancellationToken = default);

    Task SendToTenantAsync(Guid tenantId, string title, string message, string type, CancellationToken cancellationToken = default);
}
