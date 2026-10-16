namespace TaskManagementSystem.DTO
{
    public class TaskReadOnlyDTO
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public string Status { get; set; } = null!;
        public string Priority { get; set; } = null!;
        public DateTime? DueDate { get; set; }
        public int ProjectId { get; set; }
        public int? AssignedToUserId { get; set; }
        public string? AssignedToUsername { get; set; }

        public List<TaskCommentReadOnlyDTO> Comments { get; set; } = new();
        public int CommentCount { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
