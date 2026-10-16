using Microsoft.EntityFrameworkCore;
using TaskManagementSystem.Data;
using TaskManagementSystem.Repositories.Interfaces;

namespace TaskManagementSystem.Repositories
{
    public class ProjectMemberRepository : BaseRepository<ProjectMember>, IProjectMemberRepository
    {
        public ProjectMemberRepository(TaskManagementDbContext context) : base(context) { }



        public async Task<ProjectMember?> GetMemberAsync(int projectId, int userId)
        {
            return await context.ProjectMembers
                .Include(m => m.User)
                .FirstOrDefaultAsync(m =>
                    m.ProjectId == projectId &&
                    m.UserId == userId);
            // ✅ NO filter on isDeleted - gets all including deleted
        }

        // Optional: Get only active members
        public async Task<List<ProjectMember>> GetActiveMembersAsync(int projectId)
        {
            return await context.ProjectMembers
                .Include(m => m.User)
                .Where(m => m.ProjectId == projectId && !m.isDeleted)
                .ToListAsync();
        }
    }
}
