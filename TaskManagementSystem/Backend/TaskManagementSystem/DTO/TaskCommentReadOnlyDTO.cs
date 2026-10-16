namespace TaskManagementSystem.DTO
{
    public class TaskCommentReadOnlyDTO
    {
        public int Id { get; set; }
        public string Content { get; set; } = null!;
        public int UserId { get; set; }
        public string Username { get; set; } = null!;
        public DateTime CreatedDate { get; set; }
    }
}
