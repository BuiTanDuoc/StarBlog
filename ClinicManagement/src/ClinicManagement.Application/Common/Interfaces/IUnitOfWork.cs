using ClinicManagement.Domain.Common;

namespace ClinicManagement.Application.Common.Interfaces;

/// <summary>
/// Generic repository over any BaseEntity. Query() returns IQueryable so
/// handlers can compose filtering/sorting/paging (via the Shared pagination
/// helpers, Phase 3) without every specific query needing its own repository
/// method. Add specific repositories (e.g. IDoctorRepository) only when a
/// query needs logic that doesn't belong in a handler, such as availability
/// checks reused across multiple use cases.
/// </summary>
public interface IRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Untracked, filterable queryable — global tenant + soft-delete filters are already applied.</summary>
    IQueryable<T> Query();

    Task AddAsync(T entity, CancellationToken cancellationToken = default);

    void Update(T entity);

    /// <summary>Soft-deletes for AuditableEntity types (see AuditableEntitySaveChangesInterceptor); hard-deletes otherwise.</summary>
    void Remove(T entity);
}

/// <summary>
/// Aggregates repositories behind a single SaveChangesAsync so a handler can
/// touch multiple aggregates and commit them atomically in one transaction.
/// </summary>
public interface IUnitOfWork
{
    IRepository<T> Repository<T>() where T : BaseEntity;

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>Use for multi-step operations that must be all-or-nothing beyond a single SaveChanges call.</summary>
    Task<IDbContextTransactionScope> BeginTransactionAsync(CancellationToken cancellationToken = default);
}

/// <summary>Thin abstraction over EF Core's IDbContextTransaction so Application doesn't reference EF Core directly.</summary>
public interface IDbContextTransactionScope : IAsyncDisposable
{
    Task CommitAsync(CancellationToken cancellationToken = default);
    Task RollbackAsync(CancellationToken cancellationToken = default);
}
