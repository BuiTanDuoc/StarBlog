using AutoMapper;
using TaskManagementSystem.Core.Enums;
using TaskManagementSystem.Data;
using TaskManagementSystem.DTO;
using TaskManagementSystem.Exceptions;
using TaskManagementSystem.Models;
using TaskManagementSystem.Repositories.Interfaces;
using TaskManagementSystem.Services.Interfaces;

namespace TaskManagementSystem.Services
{
    public class TaskService : ITaskService
    {

        private readonly IUnitOfWork unitOfWork;
        private readonly IMapper mapper;
        private readonly ILogger<TaskService> logger;

        public TaskService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<TaskService> logger)
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
            this.logger = logger;
        }

        private async Task<bool> UserCanAccessProjectAsync(int projectId, int userId)
        {
            bool isOwner = await unitOfWork.TaskRepository.IsUserProjectOwnerAsync(projectId, userId);
            bool isMember = await unitOfWork.TaskRepository.IsUserProjectMemberAsync(projectId, userId);
            return isOwner || isMember;
        }

        public async Task<TaskCommentReadOnlyDTO> AddCommentAsync(int projectId, int taskId, int userId, TaskCommentInsertDTO dto)
        {
            if (!await UserCanAccessProjectAsync(projectId, userId))
                throw new EntityForbiddenException("You do not have permission to comment on tasks in this project");

            var task = await unitOfWork.TaskRepository.GetAsync(taskId);

            if (task == null)
                throw new EntityNotFoundException("Task", taskId);

            if (task.ProjectId != projectId)
                throw new EntityForbiddenException("Task does not belong to this project");

            // Create comment
            var comment = new TaskComment
            {
                ProjectTaskId = taskId,
                UserId = userId,
                Content = dto.Content
            };

            await unitOfWork.TaskCommentRepository.AddAsync(comment);
            await unitOfWork.SaveAsync();

            return mapper.Map<TaskCommentReadOnlyDTO>(comment);

        }

        public async Task<TaskReadOnlyDTO> AssignTaskAsync(int projectId, int taskId, int userId, int assignToUserId)
        {
            if (!await UserCanAccessProjectAsync(projectId, userId))
                throw new EntityForbiddenException("You do not have permission to assign tasks in this project");

            var task = await unitOfWork.TaskRepository.GetAsync(taskId);

            if (task == null)
                throw new EntityNotFoundException("Task", taskId);

            if (task.ProjectId != projectId)
                throw new EntityForbiddenException("Task does not belong to this project");

            // Verify user to assign is a project member
            bool isProjectMember = await unitOfWork.ProjectRepository.IsUserProjectMemberAsync(projectId, assignToUserId);
            if (!isProjectMember)
                throw new EntityForbiddenException("User is not a member of this project");

            task.AssignedToUserId = assignToUserId;
            task.ModifiedAt = DateTime.UtcNow;

            await unitOfWork.TaskRepository.UpdateAsync(task);
            await unitOfWork.SaveAsync();

            return mapper.Map<TaskReadOnlyDTO>(task);
        }

        public async Task<TaskReadOnlyDTO> CreateTaskAsync(int projectId, int userId, TaskInsertDTO dto)
        {
            if (!await UserCanAccessProjectAsync(projectId, userId))
                throw new EntityForbiddenException("You do not have permission to create tasks in this project");

            var task = mapper.Map<ProjectTask>(dto);
            task.ProjectId = projectId;

            await unitOfWork.TaskRepository.AddAsync(task);
            await unitOfWork.SaveAsync();

            return mapper.Map<TaskReadOnlyDTO>(task);

        }

        public async Task DeleteCommentAsync(int projectId, int taskId, int commentId, int userId)
        {
            if (!await UserCanAccessProjectAsync(projectId, userId))
                throw new EntityForbiddenException("You do not have permission to delete comments in this project");

            var task = await unitOfWork.TaskRepository.GetTaskWithCommentsAsync(taskId);

            if (task == null)
                throw new EntityNotFoundException("Task", taskId);

            if (task.ProjectId != projectId)
                throw new EntityForbiddenException("Task does not belong to this project");

            var comment = task.Comments.FirstOrDefault(c => c.Id == commentId);

            if (comment == null)
                throw new EntityNotFoundException("TaskComment", commentId);

            // Only comment author or project owner can delete
            bool isCommentAuthor = comment.UserId == userId;
            bool isProjectOwner = await unitOfWork.ProjectRepository.IsUserProjectOwnerAsync(projectId, userId);

            if (!isCommentAuthor && !isProjectOwner)
                throw new EntityForbiddenException("You can only delete your own comments");

            // Soft delete
            comment.isDeleted = true;
            comment.DeletedAt = DateTime.UtcNow;

            await unitOfWork.TaskCommentRepository.UpdateAsync(comment);
            await unitOfWork.SaveAsync();
        }

        public async Task DeleteTaskAsync(int projectId, int taskId, int userId)
        {
            bool isOwner = await unitOfWork.ProjectRepository.IsUserProjectOwnerAsync(projectId, userId);
            if (!isOwner)
                throw new EntityForbiddenException("Only project owner can delete tasks");

            var task = await unitOfWork.TaskRepository.GetAsync(taskId);

            if (task == null)
                throw new EntityNotFoundException("Task", taskId);

            if (task.ProjectId != projectId)
                throw new EntityForbiddenException("Task does not belong to this project");

            // Soft delete
            task.isDeleted = true;
            task.DeletedAt = DateTime.UtcNow;

            await unitOfWork.TaskRepository.UpdateAsync(task);
            await unitOfWork.SaveAsync();
        }

        public async Task<List<TaskReadOnlyDTO>> GetProjectTasksAsync(int projectId, int userId)
        {
            // Verify access
            if (!await UserCanAccessProjectAsync(projectId, userId))
                throw new EntityForbiddenException("You do not have permission to view tasks in this project");

            var tasks = await unitOfWork.TaskRepository.GetProjectTasksAsync(projectId);
            return mapper.Map<List<TaskReadOnlyDTO>>(tasks);
        }

        public async Task<TaskReadOnlyDTO> GetTaskAsync(int taskId, int userId)
        {
            var task = await unitOfWork.TaskRepository.GetTaskWithCommentsAsync(taskId);

            if (task == null)
                throw new EntityNotFoundException("Task", taskId);

            // Verify user can access the project
            if (!await UserCanAccessProjectAsync(task.ProjectId, userId))
                throw new EntityForbiddenException("You do not have permission to view this task");

            return mapper.Map<TaskReadOnlyDTO>(task);
        }

        public async Task<List<TaskCommentReadOnlyDTO>> GetTaskCommentsAsync(int projectId, int taskId, int userId)
        {
            if (!await UserCanAccessProjectAsync(projectId, userId))
                throw new EntityForbiddenException("You do not have permission to view comments in this project");

            var task = await unitOfWork.TaskRepository.GetTaskWithCommentsAsync(taskId);

            if (task == null)
                throw new EntityNotFoundException("Task", taskId);

            if (task.ProjectId != projectId)
                throw new EntityForbiddenException("Task does not belong to this project");

            var activeComments = task.Comments.Where(c => !c.isDeleted).ToList();
            return mapper.Map<List<TaskCommentReadOnlyDTO>>(activeComments);
        }

        public async Task<List<TaskReadOnlyDTO>> GetUserAssignedTasksAsync(int userId)
        {
            var tasks = await unitOfWork.TaskRepository.GetUserAssignedTasksAsync(userId);
            return mapper.Map<List<TaskReadOnlyDTO>>(tasks);
        }

        public async Task<List<TaskReadOnlyDTO>> SearchTasksAsync(int projectId, int userId, string searchTerm)
        {
            if (!await UserCanAccessProjectAsync(projectId, userId))
                throw new EntityForbiddenException("You do not have permission to search tasks in this project");

            if (string.IsNullOrWhiteSpace(searchTerm))
                throw new Exceptions.InvalidOperationException("Search term cannot be empty");

            var tasks = await unitOfWork.TaskRepository.SearchTasksAsync(projectId, searchTerm);
            return mapper.Map<List<TaskReadOnlyDTO>>(tasks);
        }

        public async Task<TaskReadOnlyDTO> UnassignTaskAsync(int projectId, int taskId, int userId)
        {
            if (!await UserCanAccessProjectAsync(projectId, userId))
                throw new EntityForbiddenException("You do not have permission to unassign tasks in this project");

            var task = await unitOfWork.TaskRepository.GetAsync(taskId);

            if (task == null)
                throw new EntityNotFoundException("Task", taskId);

            if (task.ProjectId != projectId)
                throw new EntityForbiddenException("Task does not belong to this project");

            task.AssignedToUserId = null;
            task.ModifiedAt = DateTime.UtcNow;

            await unitOfWork.TaskRepository.UpdateAsync(task);
            await unitOfWork.SaveAsync();

            return mapper.Map<TaskReadOnlyDTO>(task);
        }

        public async Task<TaskReadOnlyDTO> UpdateTaskAsync(int projectId, int taskId, int userId, TaskUpdateDTO dto)
        {
            if (!await UserCanAccessProjectAsync(projectId, userId))
                throw new EntityForbiddenException("Access denied");

            var task = await unitOfWork.TaskRepository.GetAsync(taskId);

            if (task == null || task.ProjectId != projectId)
                throw new EntityNotFoundException("Task", taskId);

            if (dto.AssignedToUserId.HasValue)
            {
                bool isProjectMember =
                    await unitOfWork.ProjectRepository
                        .IsUserProjectMemberAsync(projectId, dto.AssignedToUserId.Value);

                if (!isProjectMember)
                    throw new EntityForbiddenException("User is not a member of this project");
            }

            // Enum validation (business logic)
            if (!string.IsNullOrWhiteSpace(dto.Status))
            {
                if (!Enum.TryParse<Core.Enums.TaskStatus>(dto.Status, true, out var status))
                    throw new Exceptions.InvalidOperationException($"Invalid status: {dto.Status}");

                task.Status = status;
            }

            if (!string.IsNullOrWhiteSpace(dto.Priority))
            {
                if (!Enum.TryParse<TaskPriority>(dto.Priority, true, out var priority))
                    throw new Exceptions.InvalidOperationException($"Invalid priority: {dto.Priority}");

                task.Priority = priority;
            }


            mapper.Map(dto, task);

            task.ModifiedAt = DateTime.UtcNow;

            await unitOfWork.SaveAsync();

            return mapper.Map<TaskReadOnlyDTO>(task);
        }

        public async Task<TaskReadOnlyDTO> UpdateTaskStatusAsync(int projectId, int taskId, int userId, string newStatus)
        {
            if (!await UserCanAccessProjectAsync(projectId, userId))
                throw new EntityForbiddenException("You do not have permission to update tasks in this project");

            var task = await unitOfWork.TaskRepository.GetAsync(taskId);

            if (task == null)
                throw new EntityNotFoundException("Task", taskId);

            if (task.ProjectId != projectId)
                throw new EntityForbiddenException("Task does not belong to this project");

            // 3. Μετατροπή string σε Enum (Χρησιμοποιούμε το πλήρες όνομα λόγω του TaskStatus conflict)
            if (!Enum.TryParse<TaskManagementSystem.Core.Enums.TaskStatus>(newStatus, true, out var statusEnum))
            {
                throw new Exceptions.InvalidOperationException($"Status '{newStatus}' is not valid.");
            }

            // Update status
            task.Status = statusEnum;
            task.ModifiedAt = DateTime.UtcNow;

            await unitOfWork.TaskRepository.UpdateAsync(task);
            await unitOfWork.SaveAsync();

            return mapper.Map<TaskReadOnlyDTO>(task);
        }


        public async Task<PaginatedResult<TaskReadOnlyDTO>> GetPaginatedProjectTasksAsync(int projectId, int userId, int pageNumber, int pageSize)
        {
            // Validate access
            if (!await UserCanAccessProjectAsync(projectId, userId))
                throw new EntityForbiddenException("You do not have permission to view tasks in this project");

            // Get paginated tasks
            var tasks = await unitOfWork.TaskRepository.GetPaginatedProjectTasksAsync(projectId, pageNumber, pageSize);

            // Get total count
            var totalCount = await unitOfWork.TaskRepository.GetProjectTaskCountAsync(projectId);

            // Map to DTOs
            var taskDtos = mapper.Map<List<TaskReadOnlyDTO>>(tasks);

            // Return paginated result using your PaginatedResult class
            return new PaginatedResult<TaskReadOnlyDTO>(
                data: taskDtos,
                totalRecords: totalCount,
                pageNumber: pageNumber,
                pageSize: pageSize
            );
        }
    }
}
