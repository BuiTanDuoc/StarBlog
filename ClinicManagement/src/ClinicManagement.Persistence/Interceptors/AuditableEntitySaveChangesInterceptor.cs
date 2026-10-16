using System.Text.Json;
using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace ClinicManagement.Persistence.Interceptors;

/// <summary>
/// Runs on every SaveChanges/SaveChangesAsync call. Centralizes four cross-
/// cutting concerns so individual CQRS handlers never have to think about
/// them:
///   1. Stamps CreatedAt/By and UpdatedAt/By on AuditableEntity.
///   2. Auto-fills TenantId on new IMustHaveTenant entities from the current user.
///   3. Converts hard deletes into soft deletes (IsDeleted = true) for AuditableEntity.
///   4. Writes an AuditLog row for every Create/Update/Delete of a business entity.
/// </summary>
public class AuditableEntitySaveChangesInterceptor : SaveChangesInterceptor
{
    private readonly ICurrentUserService _currentUserService;

    public AuditableEntitySaveChangesInterceptor(ICurrentUserService currentUserService)
    {
        _currentUserService = currentUserService;
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        ProcessEntries(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        ProcessEntries(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void ProcessEntries(DbContext? context)
    {
        if (context is null) return;

        var now = DateTime.UtcNow;
        var actor = _currentUserService.UserName ?? "system";
        var auditEntries = new List<AuditLog>();

        foreach (var entry in context.ChangeTracker.Entries().ToList())
        {
            // Never audit the audit table itself, and skip entities we don't track (value objects, etc.).
            if (entry.Entity is AuditLog) continue;

            if (entry.Entity is IMustHaveTenant tenantEntity
                && entry.State == EntityState.Added
                && tenantEntity.TenantId == Guid.Empty
                && !_currentUserService.IsSuperAdmin)
            {
                tenantEntity.TenantId = _currentUserService.TenantId;
            }

            // Capture the state BEFORE the Deleted -> Modified flip below, so the audit
            // log still records this as a Delete instead of silently becoming an Update.
            var originalState = entry.State;

            if (entry.Entity is AuditableEntity auditable)
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        auditable.CreatedAt = now;
                        auditable.CreatedBy = actor;
                        break;
                    case EntityState.Modified:
                        auditable.UpdatedAt = now;
                        auditable.UpdatedBy = actor;
                        break;
                    case EntityState.Deleted:
                        // Soft delete: flip the flag and let EF persist it as an UPDATE instead of a DELETE.
                        entry.State = EntityState.Modified;
                        auditable.IsDeleted = true;
                        auditable.DeletedAt = now;
                        auditable.DeletedBy = actor;
                        break;
                }
            }

            if (originalState is EntityState.Added or EntityState.Modified or EntityState.Deleted
                && entry.Entity is BaseEntity)
            {
                auditEntries.Add(BuildAuditLog(entry, originalState, actor, now));
            }
        }

        foreach (var log in auditEntries)
        {
            context.Set<AuditLog>().Add(log);
        }
    }

    private AuditLog BuildAuditLog(EntityEntry entry, EntityState originalState, string actor, DateTime now)
    {
        var action = originalState switch
        {
            EntityState.Added => Domain.Enums.AuditAction.Create,
            EntityState.Deleted => Domain.Enums.AuditAction.Delete,
            _ => Domain.Enums.AuditAction.Update
        };

        string? oldValues = null;
        string? newValues = null;

        if (action != Domain.Enums.AuditAction.Create)
        {
            oldValues = JsonSerializer.Serialize(
                entry.OriginalValues.Properties.ToDictionary(p => p.Name, p => entry.OriginalValues[p]));
        }

        // Soft-deletes are persisted as an UPDATE (entry.State was flipped to Modified above),
        // so CurrentValues is still populated here — capture it for Delete rows too, since it
        // now reflects IsDeleted/DeletedAt/DeletedBy rather than being genuinely unavailable.
        newValues = JsonSerializer.Serialize(
            entry.CurrentValues.Properties.ToDictionary(p => p.Name, p => entry.CurrentValues[p]));

        var idProperty = entry.Properties.FirstOrDefault(p => p.Metadata.Name == "Id");

        return new AuditLog
        {
            TenantId = _currentUserService.TenantId,
            UserId = _currentUserService.UserId,
            Action = action,
            EntityName = entry.Entity.GetType().Name,
            EntityId = idProperty?.CurrentValue?.ToString() ?? string.Empty,
            OldValues = oldValues,
            NewValues = newValues,
            Timestamp = now
        };
    }
}
