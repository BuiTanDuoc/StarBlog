using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Persistence.Repositories;

public class GenericRepository<T> : IRepository<T> where T : BaseEntity
{
    private readonly ApplicationDbContext _context;
    private readonly DbSet<T> _dbSet;

    public GenericRepository(ApplicationDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _dbSet.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

    // AsNoTracking: handlers that need to mutate call Update() explicitly, which re-attaches
    // the entity — this keeps read-heavy queries (lists, filters, reports) cheap by default.
    public IQueryable<T> Query() => _dbSet.AsNoTracking();

    public async Task AddAsync(T entity, CancellationToken cancellationToken = default) =>
        await _dbSet.AddAsync(entity, cancellationToken);

    public void Update(T entity)
    {
        _dbSet.Attach(entity);
        _context.Entry(entity).State = EntityState.Modified;
    }

    public void Remove(T entity) => _dbSet.Remove(entity);
}
