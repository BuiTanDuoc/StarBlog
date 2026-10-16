using System.Collections.Concurrent;
using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Domain.Common;
using Microsoft.EntityFrameworkCore.Storage;

namespace ClinicManagement.Persistence.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private readonly ConcurrentDictionary<Type, object> _repositories = new();

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
    }

    public IRepository<T> Repository<T>() where T : BaseEntity =>
        (IRepository<T>)_repositories.GetOrAdd(typeof(T), _ => new GenericRepository<T>(_context));

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _context.SaveChangesAsync(cancellationToken);

    public async Task<IDbContextTransactionScope> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        return new DbContextTransactionScope(transaction);
    }

    private sealed class DbContextTransactionScope : IDbContextTransactionScope
    {
        private readonly IDbContextTransaction _transaction;

        public DbContextTransactionScope(IDbContextTransaction transaction) => _transaction = transaction;

        public Task CommitAsync(CancellationToken cancellationToken = default) =>
            _transaction.CommitAsync(cancellationToken);

        public Task RollbackAsync(CancellationToken cancellationToken = default) =>
            _transaction.RollbackAsync(cancellationToken);

        public ValueTask DisposeAsync() => _transaction.DisposeAsync();
    }
}
