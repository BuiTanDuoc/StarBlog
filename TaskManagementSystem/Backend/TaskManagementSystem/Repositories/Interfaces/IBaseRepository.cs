namespace TaskManagementSystem.Repositories.Interfaces
{
    public interface IBaseRepository<T>
    {
        Task AddAsync(T entity);
        Task AddRangeAsync(IEnumerable<T> entities);
        Task<IEnumerable<T>> GetAllAsync();
        Task<T?> GetAsync(int id);
        Task<int> GetCountAsync();
        Task UpdateAsync(T entity);
        Task<bool> DeleteAsync(int id);
    }
}
