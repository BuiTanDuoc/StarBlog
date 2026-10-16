namespace TaskManagementSystem.Data
{
    public class TaskComment : BaseEntity
    {
        public int Id { get; set; }
        public string Content { get; set; } = null!;

        // Foreign keys
        public int ProjectTaskId { get; set; }
        public virtual ProjectTask Task { get; set; } = null!;

        public int UserId { get; set; }
        public virtual User User { get; set; } = null!;
    }
}
