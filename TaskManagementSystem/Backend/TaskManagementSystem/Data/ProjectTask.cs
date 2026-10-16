using TaskManagementSystem.Core.Enums;

namespace TaskManagementSystem.Data
{
    public class ProjectTask : BaseEntity
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public Core.Enums.TaskStatus Status { get; set; }
        public TaskPriority Priority { get; set; } = TaskPriority.Medium;

        // Dates
        public DateTime? DueDate { get; set; }

        // Foreign keys
        public int ProjectId { get; set; }
        public virtual Project Project { get; set; } = null!;

        public int? AssignedToUserId { get; set; } // nullable - task may not be assigned
        public virtual User? AssignedToUser { get; set; }

        // Navigation properties
        public virtual ICollection<TaskComment> Comments { get; set; } = new HashSet<TaskComment>();
    }
}
