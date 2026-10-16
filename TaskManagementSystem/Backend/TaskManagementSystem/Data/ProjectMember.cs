using TaskManagementSystem.Core.Enums;

namespace TaskManagementSystem.Data
{
    public class ProjectMember : BaseEntity
    {
        public int Id { get; set; }

        // Foreign keys
        public int ProjectId { get; set; }
        public virtual Project Project { get; set; } = null!;

        public int UserId { get; set; }
        public virtual User User { get; set; } = null!;

        // Role in project
        public ProjectRole Role { get; set; } = ProjectRole.Member;
    }
}
