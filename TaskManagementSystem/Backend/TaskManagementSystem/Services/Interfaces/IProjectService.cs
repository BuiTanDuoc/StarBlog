using TaskManagementSystem.DTO;

namespace TaskManagementSystem.Services.Interfaces
{
    public interface IProjectService
    {
        // ============= PROJECT CRUD =============
        /// <summary>
        /// Creates a new project for the authenticated user
        /// </summary>
        Task<ProjectReadOnlyDTO> CreateProjectAsync(int userId, ProjectInsertDTO dto);

        /// <summary>
        /// Gets a single project by ID with full details (members and tasks)
        /// </summary>
        Task<ProjectReadOnlyDTO> GetProjectAsync(int projectId, int userId);

        /// <summary>
        /// Gets all projects for the authenticated user (owned or member of)
        /// </summary>
        Task<List<ProjectReadOnlyDTO>> GetUserProjectsAsync(int userId);

        /// <summary>
        /// Updates project name and description
        /// </summary>
        Task<ProjectReadOnlyDTO> UpdateProjectAsync(int projectId, int userId, ProjectUpdateDTO dto);

        /// <summary>
        /// Deletes a project (only owner can delete)
        /// </summary>
        Task DeleteProjectAsync(int projectId, int userId);

        // ============= PROJECT MEMBERS =============
        /// <summary>
        /// Adds a user to a project as a member
        /// </summary>
        Task<ProjectMemberReadOnlyDTO> AddProjectMemberAsync(int projectId, int userId, AddProjectMemberDTO dto);

        /// <summary>
        /// Removes a user from a project
        /// </summary>
        Task RemoveProjectMemberAsync(int projectId, int memberId, int userId);

        /// <summary>
        /// Updates a member's role in the project
        /// </summary>
        Task<ProjectMemberReadOnlyDTO> UpdateProjectMemberRoleAsync(int projectId, int memberId, string newRole, int userId);

        /// <summary>
        /// Gets all members of a project
        /// </summary>
        Task<List<ProjectMemberReadOnlyDTO>> GetProjectMembersAsync(int projectId, int userId);
    }
}
