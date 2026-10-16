using System.Linq.Expressions;
using TaskManagementSystem.Data;

namespace TaskManagementSystem.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetUserAsync(string username, string password);
        Task<List<User>> SearchByUsernameAsync(string username);
        Task<Models.PaginatedResult<User>> GetUsersAsync(int pageNumber, int pageSize,
           List<Expression<Func<User, bool>>> predicates);
    }
}
