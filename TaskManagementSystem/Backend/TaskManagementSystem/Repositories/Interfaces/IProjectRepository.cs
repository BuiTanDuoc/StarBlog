using TaskManagementSystem.Data;

namespace TaskManagementSystem.Repositories.Interfaces
{
    public interface IProjectRepository : IBaseRepository<Project>
    {
        Task<List<Project>> GetUserProjectsAsync(int userId);
        Task<Project?> GetProjectWithMembersAsync(int projectId);
        Task<Project?> GetProjectWithTasksAsync(int projectId);
        Task<bool> IsUserProjectOwnerAsync(int projectId, int userId);
        Task<bool> IsUserProjectMemberAsync(int projectId, int userId);
    }
}
