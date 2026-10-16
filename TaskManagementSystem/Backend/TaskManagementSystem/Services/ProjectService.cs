using AutoMapper;
using TaskManagementSystem.Data;
using TaskManagementSystem.DTO;
using TaskManagementSystem.Exceptions;
using TaskManagementSystem.Repositories.Interfaces;
using TaskManagementSystem.Services.Interfaces;

namespace TaskManagementSystem.Services
{
    public class ProjectService : IProjectService
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IMapper mapper;
        private readonly ILogger logger;

        public ProjectService(IUnitOfWork unitOfWork, IMapper mapper, ILogger logger)
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
            this.logger = logger;
        }

        public async Task<ProjectMemberReadOnlyDTO> AddProjectMemberAsync(int projectId, int userId, AddProjectMemberDTO dto)
        {
            // ✅ Verify project exists
            var project = await unitOfWork.ProjectRepository.GetAsync(projectId);
            if (project == null)
                throw new EntityNotFoundException("Project", projectId);

            // ✅ Verify user can add members (owner only)
            if (project.OwnerId != userId)
                throw new EntityForbiddenException("Only project owner can add members");

            // ✅ Verify user to add exists
            var userToAdd = await unitOfWork.UserRepository.GetAsync(dto.UserId);
            if (userToAdd == null)
                throw new EntityNotFoundException("User", dto.UserId);

            // ✅ Check if member exists (including deleted)
            var existingMember = await unitOfWork.ProjectMemberRepository
                .GetMemberAsync(projectId, dto.UserId);

            if (existingMember != null && !existingMember.isDeleted)
            {
                // Member already active
                throw new EntityAlreadyExistsException("User", "User is already a member of this project");
            }

            if (existingMember != null && existingMember.isDeleted)
            {
                // ✅ RESTORE deleted member instead of creating new
                logger.LogInformation("Restoring soft-deleted member {UserId} to project {ProjectId}",
                    dto.UserId, projectId);

                existingMember.isDeleted = false;
                existingMember.DeletedAt = null;
                existingMember.Role = Enum.Parse<Core.Enums.ProjectRole>(dto.Role);
                existingMember.ModifiedAt = DateTime.UtcNow;

                await unitOfWork.ProjectMemberRepository.UpdateAsync(existingMember);
            }
            else
            {
                // ✅ Create new member
                logger.LogInformation("Adding new member {UserId} to project {ProjectId}",
                    dto.UserId, projectId);

                var member = new ProjectMember
                {
                    ProjectId = projectId,
                    UserId = dto.UserId,
                    Role = Enum.Parse<Core.Enums.ProjectRole>(dto.Role)
                };

                await unitOfWork.ProjectMemberRepository.AddAsync(member);
            }

            await unitOfWork.SaveAsync();

            // ✅ Get the member with user info
            var memberToReturn = await unitOfWork.ProjectMemberRepository
                .GetMemberAsync(projectId, dto.UserId);

            if (memberToReturn == null)
                throw new EntityNotFoundException("ProjectMember", dto.UserId);

            logger.LogInformation("Successfully added/restored member {UserId} to project {ProjectId}",
                dto.UserId, projectId);

            return mapper.Map<ProjectMemberReadOnlyDTO>(memberToReturn);
        }

        public async Task<ProjectReadOnlyDTO> CreateProjectAsync(int userId, ProjectInsertDTO dto)
        {
            var project = mapper.Map<Project>(dto);
            project.OwnerId = userId;

            await unitOfWork.ProjectRepository.AddAsync(project);
            await unitOfWork.SaveAsync();

            // Add the project creator as an "Owner" member
            var ownerMember = new ProjectMember
            {
                ProjectId = project.Id,
                UserId = userId,
                Role = Core.Enums.ProjectRole.Owner
            };

            await unitOfWork.ProjectMemberRepository.AddAsync(ownerMember);
            await unitOfWork.SaveAsync();

            return mapper.Map<ProjectReadOnlyDTO>(project);
        }

        public async Task DeleteProjectAsync(int projectId, int userId)
        {
            var project = await unitOfWork.ProjectRepository.GetAsync(projectId);
            if (project == null)
                throw new EntityNotFoundException("Project", projectId);

            if (project.OwnerId != userId)
                throw new EntityForbiddenException("Only project owner can delete");
            project.isDeleted = true;
            project.DeletedAt = DateTime.UtcNow;

            await unitOfWork.ProjectRepository.UpdateAsync(project);
            await unitOfWork.SaveAsync();

        }

        public async Task<ProjectReadOnlyDTO> GetProjectAsync(int projectId, int userId)
        {
            var project = await unitOfWork.ProjectRepository.GetProjectWithMembersAsync(projectId);

            if (project == null)
            {
                throw new EntityNotFoundException("Project", projectId);
            }
            //check if use is owner or member
            bool isOwner = project.OwnerId == userId;
            bool isMember = project.Members.Any(m => m.UserId == userId);

            if (!isOwner && !isMember)
                throw new EntityForbiddenException("You do not have permission to view this project");

            var projectWithTasks = await unitOfWork.ProjectRepository.GetProjectWithTasksAsync(projectId);
            return mapper.Map<ProjectReadOnlyDTO>(projectWithTasks);
        }

        public async Task<List<ProjectMemberReadOnlyDTO>> GetProjectMembersAsync(int projectId, int userId)
        {
            var project = await unitOfWork.ProjectRepository.GetProjectWithMembersAsync(projectId);

            if (project == null)
                throw new EntityNotFoundException("Project", projectId);

            // Check access
            bool isOwner = project.OwnerId == userId;
            bool isMember = project.Members.Any(m => m.UserId == userId && !m.isDeleted);

            if (!isOwner && !isMember)
                throw new EntityForbiddenException("You do not have permission to view project members");

            // ✅ Return only active members
            var activeMembers = project.Members.Where(m => !m.isDeleted).ToList();

            return mapper.Map<List<ProjectMemberReadOnlyDTO>>(activeMembers);
        }

        public async Task<List<ProjectReadOnlyDTO>> GetUserProjectsAsync(int userId)
        {
            var projects = await unitOfWork.ProjectRepository.GetUserProjectsAsync(userId);
            return mapper.Map<List<ProjectReadOnlyDTO>>(projects);
        }

        public async Task RemoveProjectMemberAsync(int projectId, int memberId, int userId)
        {
            var project = await unitOfWork.ProjectRepository.GetProjectWithMembersAsync(projectId);

            if (project == null)
                throw new EntityNotFoundException("Project", projectId);

            // Only owner can remove members
            if (project.OwnerId != userId)
                throw new EntityForbiddenException("Only project owner can remove members");


            // Find member
            var member = project.Members.FirstOrDefault(m => m.Id == memberId);

            if (member == null)
                throw new EntityNotFoundException("ProjectMember", memberId);

            if (member.UserId == project.OwnerId)
                throw new Exceptions.InvalidOperationException("The owner cannot be removed from the project.");


            // Soft delete the member
            member.isDeleted = true;
            member.DeletedAt = DateTime.UtcNow;

            await unitOfWork.ProjectMemberRepository.UpdateAsync(member);
            await unitOfWork.SaveAsync();
        }

        public async Task<ProjectReadOnlyDTO> UpdateProjectAsync(int projectId, int userId, ProjectUpdateDTO dto)
        {
            var project = await unitOfWork.ProjectRepository.GetAsync(projectId);

            if (project == null)
                throw new EntityNotFoundException("Project", projectId);

            if (project.OwnerId != userId)
                throw new EntityForbiddenException("Only project owner can update");

            if (!string.IsNullOrEmpty(dto.Name))
                project.Name = dto.Name;

            if (dto.Description != null)
                project.Description = dto.Description;

            project.ModifiedAt = DateTime.UtcNow;

            await unitOfWork.ProjectRepository.UpdateAsync(project);
            await unitOfWork.SaveAsync();

            return mapper.Map<ProjectReadOnlyDTO>(project);
        }

        public async Task<ProjectMemberReadOnlyDTO> UpdateProjectMemberRoleAsync(int projectId, int memberId, string newRole, int userId)
        {
            var project = await unitOfWork.ProjectRepository.GetProjectWithMembersAsync(projectId);

            if (project == null)
                throw new EntityNotFoundException("Project", projectId);

            // Only owner can update roles
            if (project.OwnerId != userId)
                throw new EntityForbiddenException("Only project owner can update member roles");

            var member = project.Members.FirstOrDefault(m => m.Id == memberId);

            if (member == null)
                throw new EntityNotFoundException("ProjectMember", memberId);

            // Update role
            member.Role = Enum.Parse<Core.Enums.ProjectRole>(newRole);
            member.ModifiedAt = DateTime.UtcNow;

            await unitOfWork.ProjectMemberRepository.UpdateAsync(member);
            await unitOfWork.SaveAsync();

            return mapper.Map<ProjectMemberReadOnlyDTO>(member);
        }
    }
}
