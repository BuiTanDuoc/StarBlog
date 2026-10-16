namespace TaskManagementSystem.DTO
{
    public class ProjectReadOnlyDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public int OwnerId { get; set; }
        public string OwnerName { get; set; } = null!;
        public int MemberCount { get; set; }
        public int TaskCount { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
