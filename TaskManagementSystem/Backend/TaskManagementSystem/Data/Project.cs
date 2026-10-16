namespace TaskManagementSystem.Data
{
    public class Project : BaseEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }

        // Foreign key - Project owner
        public int OwnerId { get; set; }
        public virtual User Owner { get; set; } = null!;

        // Navigation properties
        public virtual ICollection<ProjectMember> Members { get; set; } = new HashSet<ProjectMember>();
        public virtual ICollection<ProjectTask> Tasks { get; set; } = new HashSet<ProjectTask>();
    }
}
