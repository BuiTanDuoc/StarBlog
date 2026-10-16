using Microsoft.EntityFrameworkCore;
using TaskManagementSystem.Data;
using TaskManagementSystem.Repositories.Interfaces;

namespace TaskManagementSystem.Repositories
{
    public class TaskRepository : BaseRepository<ProjectTask>, ITaskRepository
    {
        public TaskRepository(TaskManagementDbContext context) : base(context) { }


        public async Task<bool> IsUserProjectOwnerAsync(int projectId, int userId)
        {
            // Χρησιμοποιούμε AnyAsync για να πάρει η SQL μόνο ένα bit πληροφορίας
            return await context.Projects
                .AnyAsync(p => p.Id == projectId && p.OwnerId == userId);
        }

        // Ελέγχει αν ο χρήστης είναι απλό μέλος ή ο ιδιοκτήτης 
        // (Συνήθως ο ιδιοκτήτης θεωρείται "μέλος" με όλα τα δικαιώματα)
        public async Task<bool> IsUserProjectMemberAsync(int projectId, int userId)
        {
            // Ελέγχουμε απευθείας στον πίνακα ProjectMembers που είναι πιο μικρός και γρήγορος
            return await context.ProjectMembers
                .AnyAsync(pm => pm.ProjectId == projectId && pm.UserId == userId && !pm.isDeleted);
        }

        public async Task<List<ProjectTask>> GetProjectTasksAsync(int projectId)
        {
            return await context.Tasks
                .Where(t => t.ProjectId == projectId && !t.isDeleted)
                .Include(t => t.AssignedToUser)
                .OrderByDescending(t => t.InsertedAt)
                .ToListAsync();
        }

        public async Task<ProjectTask?> GetTaskWithCommentsAsync(int taskId)
        {
            return await context.Tasks
                .Include(t => t.Comments)
                    .ThenInclude(c => c.User)
                .Include(t => t.AssignedToUser)
                .FirstOrDefaultAsync(t => t.Id == taskId && !t.isDeleted);
        }

        public async Task<List<ProjectTask>> GetUserAssignedTasksAsync(int userId)
        {
            return await context.Tasks
                .Where(t => t.AssignedToUserId == userId && !t.isDeleted)
                .Include(t => t.Project)
                .OrderByDescending(t => t.InsertedAt)
                .ToListAsync();
        }

        public async Task<List<ProjectTask>> SearchTasksAsync(int projectId, string searchTerm)
        {
            return await context.Tasks
                .Where(t => t.ProjectId == projectId && !t.isDeleted &&
                           (t.Title.ToLower().Contains(searchTerm.ToLower()) ||
                            t.Description!.ToLower().Contains(searchTerm.ToLower())))
                .Include(t => t.AssignedToUser)
                .OrderByDescending(t => t.InsertedAt)
                .ToListAsync();
        }
        public async Task<List<ProjectTask>> GetPaginatedProjectTasksAsync(int projectId, int pageNumber, int pageSize)
        {
            return await context.Tasks
                .Where(t => t.ProjectId == projectId && !t.isDeleted)
                .Include(t => t.AssignedToUser)
                .OrderByDescending(t => t.InsertedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetProjectTaskCountAsync(int projectId)
        {
            return await context.Tasks
                .Where(t => t.ProjectId == projectId && !t.isDeleted)
                .CountAsync();
        }

    }
}

