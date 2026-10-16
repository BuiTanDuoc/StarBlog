using TaskManagementSystem.Data;
using TaskManagementSystem.Repositories.Interfaces;

namespace TaskManagementSystem.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly TaskManagementDbContext _context;
        public UnitOfWork(TaskManagementDbContext context)
        {
            _context = context;
        }


        public UserRepository UserRepository => new(_context);
        public ProjectRepository ProjectRepository => new(_context);
        public TaskRepository TaskRepository => new(_context);
        public ProjectMemberRepository ProjectMemberRepository => new(_context);
        public TaskCommentRepository TaskCommentRepository => new(_context);

        public async Task<bool> SaveAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
