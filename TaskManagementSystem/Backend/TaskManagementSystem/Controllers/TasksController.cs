using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManagementSystem.DTO;
using TaskManagementSystem.Models;
using TaskManagementSystem.Services.Interfaces;

namespace TaskManagementSystem.Controllers
{
    /// <summary>
    /// Manages task operations including creation, updates, deletion, status changes, and comments.
    /// All endpoints require authentication via JWT token.
    /// </summary>
    [Route("api/projects/{projectId}/[controller]")]
    [ApiController]
    [Authorize]
    public class TasksController : BaseController
    {


        public TasksController(IApplicationService applicationService)
            : base(applicationService)
        {

        }


        /// <summary>
        /// Creates a new task in the project.
        /// </summary>
        [HttpPost("create")]
        public async Task<ActionResult<TaskReadOnlyDTO>> CreateTask(int projectId, [FromBody] TaskInsertDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await applicationService.TaskService.CreateTaskAsync(projectId, AppUser!.Id, dto);
            return CreatedAtAction(nameof(GetTask), new { projectId, id = result.Id }, result);
        }

        /// <summary>
        /// Retrieves a single task with all its comments.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<TaskReadOnlyDTO>> GetTask(int projectId, int id)
        {
            var result = await applicationService.TaskService.GetTaskAsync(id, AppUser!.Id);
            return Ok(result);
        }

        /// <summary>
        /// Retrieves all tasks in the project.
        /// </summary>
        [HttpGet("all")]
        public async Task<ActionResult<List<TaskReadOnlyDTO>>> GetProjectTasks(int projectId)
        {
            var result = await applicationService.TaskService.GetProjectTasksAsync(projectId, AppUser!.Id);
            return Ok(result);
        }

        /// <summary>
        /// Retrieves all tasks assigned to the current user.
        /// </summary>
        [HttpGet("assigned")]
        public async Task<ActionResult<List<TaskReadOnlyDTO>>> GetUserAssignedTasks()
        {
            var result = await applicationService.TaskService.GetUserAssignedTasksAsync(AppUser!.Id);
            return Ok(result);
        }

        /// <summary>
        /// Searches for tasks in a project by title or description.
        /// </summary>
        [HttpGet("search")]
        public async Task<ActionResult<List<TaskReadOnlyDTO>>> SearchTasks(int projectId, [FromQuery] string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return BadRequest(new { message = "Search query cannot be empty." });

            var result = await applicationService.TaskService.SearchTasksAsync(projectId, AppUser!.Id, query);
            return Ok(result);
        }

        /// <summary>
        /// Updates task details (title, description, status, priority, due date, assignment).
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<TaskReadOnlyDTO>> UpdateTask(int projectId, int id, [FromBody] TaskUpdateDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await applicationService.TaskService.UpdateTaskAsync(projectId, id, AppUser!.Id, dto);
            return Ok(result);
        }

        /// <summary>
        /// Changes the status of a task (Backlog, InProgress, Review, Done).
        /// </summary>
        [HttpPatch("{id}/status")]
        public async Task<ActionResult<TaskReadOnlyDTO>> UpdateTaskStatus(int projectId, int id, [FromQuery] string newStatus)
        {
            if (string.IsNullOrWhiteSpace(newStatus))
                return BadRequest(new { message = "Status cannot be empty." });

            var result = await applicationService.TaskService.UpdateTaskStatusAsync(projectId, id, AppUser!.Id, newStatus);
            return Ok(result);
        }

        /// <summary>
        /// Assigns a task to a project member.
        /// </summary>
        [HttpPut("{id}/assign")]
        public async Task<ActionResult<TaskReadOnlyDTO>> AssignTask(int projectId, int id, [FromQuery] int assignToUserId)
        {
            var result = await applicationService.TaskService.AssignTaskAsync(projectId, id, AppUser!.Id, assignToUserId);
            return Ok(result);
        }

        /// <summary>
        /// Unassigns a task (removes the assignment).
        /// </summary>
        [HttpPut("{id}/unassign")]
        public async Task<ActionResult<TaskReadOnlyDTO>> UnassignTask(int projectId, int id)
        {
            var result = await applicationService.TaskService.UnassignTaskAsync(projectId, id, AppUser!.Id);
            return Ok(result);
        }

        /// <summary>
        /// Deletes a task.
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTask(int projectId, int id)
        {
            await applicationService.TaskService.DeleteTaskAsync(projectId, id, AppUser!.Id);
            return Ok(new { message = "Task deleted successfully." });
        }

        // ============= TASK COMMENTS =============

        /// <summary>
        /// Adds a comment to a task.
        /// </summary>
        [HttpPost("{taskId}/comments")]
        public async Task<ActionResult<TaskCommentReadOnlyDTO>> AddComment(int projectId, int taskId, [FromBody] TaskCommentInsertDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await applicationService.TaskService.AddCommentAsync(projectId, taskId, AppUser!.Id, dto);
            return CreatedAtAction(nameof(GetTaskComments), new { projectId, taskId }, result);
        }

        /// <summary>
        /// Retrieves all comments on a task.
        /// </summary>
        [HttpGet("{taskId}/comments")]
        public async Task<ActionResult<List<TaskCommentReadOnlyDTO>>> GetTaskComments(int projectId, int taskId)
        {
            var result = await applicationService.TaskService.GetTaskCommentsAsync(projectId, taskId, AppUser!.Id);
            return Ok(result);
        }

        /// <summary>
        /// Deletes a comment.
        /// </summary>
        [HttpDelete("{taskId}/comments/{commentId}")]
        public async Task<IActionResult> DeleteComment(int projectId, int taskId, int commentId)
        {
            await applicationService.TaskService.DeleteCommentAsync(projectId, taskId, commentId, AppUser!.Id);
            return Ok(new { message = "Comment deleted successfully." });
        }

        /// <summary>
        /// Retrieves paginated tasks in the project.
        /// </summary>
        [HttpGet("paginated")]
        public async Task<ActionResult<PaginatedResult<TaskReadOnlyDTO>>> GetPaginatedProjectTasks(
        int projectId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
        {
            if (pageNumber < 1 || pageSize < 1)
                return BadRequest(new { message = "Page number and page size must be greater than 0." });
            var result = await applicationService.TaskService.GetPaginatedProjectTasksAsync(projectId, AppUser!.Id, pageNumber, pageSize);
            return Ok(result);
        }
    }
}
