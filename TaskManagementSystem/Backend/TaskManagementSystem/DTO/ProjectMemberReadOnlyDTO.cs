namespace TaskManagementSystem.DTO
{
    public class ProjectMemberReadOnlyDTO
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Username { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Role { get; set; } = null!;
    }
}
