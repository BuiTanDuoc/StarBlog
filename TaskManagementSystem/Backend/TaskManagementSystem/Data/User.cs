using TaskManagementSystem.Core.Enums;

namespace TaskManagementSystem.Data
{
    public class User : BaseEntity
    {
        public int Id { get; set; }
        public string Username { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string Firstname { get; set; } = null!;
        public string Lastname { get; set; } = null!;
        public UserRole UserRole { get; set; }

        // Navigation properties
        public virtual ICollection<Project> OwnedProjects { get; set; } = new HashSet<Project>();
        public virtual ICollection<ProjectMember> ProjectMemberships { get; set; } = new HashSet<ProjectMember>();
        public virtual ICollection<ProjectTask> AssignedTasks { get; set; } = new HashSet<ProjectTask>();
        public virtual ICollection<TaskComment> Comments { get; set; } = new HashSet<TaskComment>();
    }
}
