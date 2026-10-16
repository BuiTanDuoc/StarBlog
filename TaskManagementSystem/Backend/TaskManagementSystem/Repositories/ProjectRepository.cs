using Microsoft.EntityFrameworkCore;
using TaskManagementSystem.Data;
using TaskManagementSystem.Repositories.Interfaces;

namespace TaskManagementSystem.Repositories
{
    public class ProjectRepository : BaseRepository<Project>, IProjectRepository
    {
        public ProjectRepository(TaskManagementDbContext context) : base(context) { }

        public async Task<List<Project>> GetUserProjectsAsync(int userId)
        {
            return await context.Projects
                .Where(p => !p.isDeleted && (p.OwnerId == userId || p.Members.Any(m => m.UserId == userId && !m.isDeleted)))
                .OrderByDescending(p => p.InsertedAt)
                .ToListAsync();
        }

        public async Task<Project?> GetProjectWithMembersAsync(int projectId)
        {
            return await context.Projects
                .Include(p => p.Owner)
                .Include(p => p.Members)
                    .ThenInclude(m => m.User)
                .FirstOrDefaultAsync(p => p.Id == projectId && !p.isDeleted);
            // ✅ NO filter on m.isDeleted here - load all
        }

        public async Task<Project?> GetProjectWithTasksAsync(int projectId)
        {
            return await context.Projects
                    .Include(p => p.Tasks.Where(t => !t.isDeleted))
                    .ThenInclude(t => t.AssignedToUser)
                    .Include(p => p.Tasks.Where(t => !t.isDeleted))
                    .ThenInclude(t => t.Comments.Where(c => !c.isDeleted))
                    .FirstOrDefaultAsync(p => p.Id == projectId && !p.isDeleted);
        }

        public async Task<bool> IsUserProjectOwnerAsync(int projectId, int userId)
        {
            return await context.Projects
                .AnyAsync(p => p.Id == projectId && p.OwnerId == userId);
        }

        public async Task<bool> IsUserProjectMemberAsync(int projectId, int userId)
        {
            return await context.ProjectMembers
                .AnyAsync(pm => pm.ProjectId == projectId && pm.UserId == userId);
        }


    }
}
