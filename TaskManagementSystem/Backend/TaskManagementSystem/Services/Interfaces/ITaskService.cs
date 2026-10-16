using TaskManagementSystem.DTO;

namespace TaskManagementSystem.Services.Interfaces
{
    public interface ITaskService
    {
        // ============= TASK CRUD =============
        /// <summary>
        /// Creates a new task in a project
        /// </summary>
        Task<TaskReadOnlyDTO> CreateTaskAsync(int projectId, int userId, TaskInsertDTO dto);

        /// <summary>
        /// Gets a single task with all details (comments, assigned user, etc.)
        /// </summary>
        Task<TaskReadOnlyDTO> GetTaskAsync(int taskId, int userId);

        /// <summary>
        /// Gets all tasks in a project
        /// </summary>
        Task<List<TaskReadOnlyDTO>> GetProjectTasksAsync(int projectId, int userId);

        /// <summary>
        /// Gets all tasks assigned to the current user
        /// </summary>
        Task<List<TaskReadOnlyDTO>> GetUserAssignedTasksAsync(int userId);

        /// <summary>
        /// Updates task details (title, description, status, priority, due date, assignment)
        /// </summary>
        Task<TaskReadOnlyDTO> UpdateTaskAsync(int projectId, int taskId, int userId, TaskUpdateDTO dto);

        /// <summary>
        /// Changes task status (Backlog, InProgress, Review, Done)
        /// </summary>
        Task<TaskReadOnlyDTO> UpdateTaskStatusAsync(int projectId, int taskId, int userId, string newStatus);

        /// <summary>
        /// Assigns a task to a project member
        /// </summary>
        Task<TaskReadOnlyDTO> AssignTaskAsync(int projectId, int taskId, int userId, int assignToUserId);

        /// <summary>
        /// Unassigns a task (clears the assignment)
        /// </summary>
        Task<TaskReadOnlyDTO> UnassignTaskAsync(int projectId, int taskId, int userId);

        /// <summary>
        /// Deletes a task (only project owner or task assigner can delete)
        /// </summary>
        Task DeleteTaskAsync(int projectId, int taskId, int userId);

        /// <summary>
        /// Searches tasks in a project by title or description
        /// </summary>
        Task<List<TaskReadOnlyDTO>> SearchTasksAsync(int projectId, int userId, string searchTerm);

        // ============= TASK COMMENTS =============
        /// <summary>
        /// Adds a comment to a task
        /// </summary>
        Task<TaskCommentReadOnlyDTO> AddCommentAsync(int projectId, int taskId, int userId, TaskCommentInsertDTO dto);

        /// <summary>
        /// Gets all comments on a task
        /// </summary>
        Task<List<TaskCommentReadOnlyDTO>> GetTaskCommentsAsync(int projectId, int taskId, int userId);

        /// <summary>
        /// Deletes a comment (only author or project owner can delete)
        /// </summary>
        Task DeleteCommentAsync(int projectId, int taskId, int commentId, int userId);
    }
}
