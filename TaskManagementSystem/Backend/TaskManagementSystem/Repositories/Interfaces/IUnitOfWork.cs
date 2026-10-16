namespace TaskManagementSystem.Repositories.Interfaces
{

    public interface IUnitOfWork
    {
        // ✅ Use concrete classes like your old app
        UserRepository UserRepository { get; }
        ProjectRepository ProjectRepository { get; }
        TaskRepository TaskRepository { get; }
        ProjectMemberRepository ProjectMemberRepository { get; }
        TaskCommentRepository TaskCommentRepository { get; }
        Task<bool> SaveAsync();
    }
}
