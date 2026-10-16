using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManagementSystem.DTO;
using TaskManagementSystem.Services.Interfaces;

namespace TaskManagementSystem.Controllers
{
    /// <summary>
    /// Manages project operations including creation, updates, deletion, and member management.
    /// All endpoints require authentication via JWT token.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProjectsController : BaseController
    {


        public ProjectsController(IApplicationService applicationService)
            : base(applicationService)
        {

        }


        /// <summary>
        /// Creates a new project. The authenticated user becomes the project owner.
        /// </summary>
        [HttpPost("create")]
        public async Task<ActionResult<ProjectReadOnlyDTO>> CreateProject([FromBody] ProjectInsertDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await applicationService.ProjectService.CreateProjectAsync(AppUser!.Id, dto);
            return CreatedAtAction(nameof(GetProject), new { id = result.Id }, result);
        }

        /// <summary>
        /// Retrieves a single project with all its members and tasks.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ProjectReadOnlyDTO>> GetProject(int id)
        {
            var result = await applicationService.ProjectService.GetProjectAsync(id, AppUser!.Id);
            return Ok(result);
        }

        /// <summary>
        /// Retrieves all projects owned by or that the user is a member of.
        /// </summary>
        [HttpGet("user/all")]
        public async Task<ActionResult<List<ProjectReadOnlyDTO>>> GetUserProjects()
        {
            var result = await applicationService.ProjectService.GetUserProjectsAsync(AppUser!.Id);
            return Ok(result);
        }

        /// <summary>
        /// Updates project name and/or description.
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<ProjectReadOnlyDTO>> UpdateProject(int id, [FromBody] ProjectUpdateDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await applicationService.ProjectService.UpdateProjectAsync(id, AppUser!.Id, dto);
            return Ok(result);
        }

        /// <summary>
        /// Soft deletes a project.
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProject(int id)
        {
            await applicationService.ProjectService.DeleteProjectAsync(id, AppUser!.Id);
            return Ok(new { message = "Project deleted successfully." });
        }

        // ============= PROJECT MEMBERS =============

        /// <summary>
        /// Adds a user to the project as a member with a specific role.
        /// </summary>
        [HttpPost("{projectId}/members")]
        public async Task<ActionResult<ProjectMemberReadOnlyDTO>> AddProjectMember(int projectId, [FromBody] AddProjectMemberDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await applicationService.ProjectService.AddProjectMemberAsync(projectId, AppUser!.Id, dto);
            return CreatedAtAction(nameof(GetProjectMembers), new { projectId }, result);
        }

        /// <summary>
        /// Retrieves all members of a project.
        /// </summary>
        [HttpGet("{projectId}/members")]
        public async Task<ActionResult<List<ProjectMemberReadOnlyDTO>>> GetProjectMembers(int projectId)
        {
            var result = await applicationService.ProjectService.GetProjectMembersAsync(projectId, AppUser!.Id);
            return Ok(result);
        }

        /// <summary>
        /// Updates a member's role in the project.
        /// </summary>
        [HttpPut("{projectId}/members/{memberId}/role")]
        public async Task<ActionResult<ProjectMemberReadOnlyDTO>> UpdateProjectMemberRole(int projectId, int memberId, [FromQuery] string newRole)
        {
            if (string.IsNullOrWhiteSpace(newRole))
                return BadRequest(new { message = "Role cannot be empty." });

            var result = await applicationService.ProjectService.UpdateProjectMemberRoleAsync(projectId, memberId, newRole, AppUser!.Id);
            return Ok(result);
        }

        /// <summary>
        /// Removes a user from the project.
        /// </summary>
        [HttpDelete("{projectId}/members/{memberId}")]
        public async Task<IActionResult> RemoveProjectMember(int projectId, int memberId)
        {
            await applicationService.ProjectService.RemoveProjectMemberAsync(projectId, memberId, AppUser!.Id);
            return Ok(new { message = "Member removed from project." });
        }
    }
}
