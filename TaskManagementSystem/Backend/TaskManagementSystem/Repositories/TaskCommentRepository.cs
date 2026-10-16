using TaskManagementSystem.Data;
using TaskManagementSystem.Repositories.Interfaces;

namespace TaskManagementSystem.Repositories
{
    public class TaskCommentRepository : BaseRepository<TaskComment>, ITaskCommentRepository
    {
        public TaskCommentRepository(TaskManagementDbContext context) : base(context) { }
    }
}