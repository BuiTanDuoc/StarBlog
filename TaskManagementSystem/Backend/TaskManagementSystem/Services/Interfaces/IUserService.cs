using TaskManagementSystem.Core.Filters;
using TaskManagementSystem.Data;
using TaskManagementSystem.DTO;
using TaskManagementSystem.Models;

namespace TaskManagementSystem.Services.Interfaces
{
    public interface IUserService
    {
        //
        Task<UserReadOnlyDTO?> GetUserByIdAsync(int id);
        Task<User?> VerifyAndGetUserAsync(UserLoginDTO credentials);
        Task<UserReadOnlyDTO?> GetUserByUsernameAsync(string username);
        Task<PaginatedResult<UserReadOnlyDTO>> GetPaginatedUsersFilteredAsync(int pageNumber, int pageSize, UserFiltersDTO userFiltersDTO);
    }
}
