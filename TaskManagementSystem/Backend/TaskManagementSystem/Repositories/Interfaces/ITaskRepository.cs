using TaskManagementSystem.Data;

namespace TaskManagementSystem.Repositories.Interfaces
{
    public interface ITaskRepository : IBaseRepository<ProjectTask>
    {
        Task<List<ProjectTask>> GetProjectTasksAsync(int projectId);
        Task<ProjectTask?> GetTaskWithCommentsAsync(int taskId);
        Task<List<ProjectTask>> GetUserAssignedTasksAsync(int userId);
        Task<List<ProjectTask>> SearchTasksAsync(int projectId, string searchTerm);
        Task<List<ProjectTask>> GetPaginatedProjectTasksAsync(int projectId, int pageNumber, int pageSize);
        Task<int> GetProjectTaskCountAsync(int projectId);
    }
}
