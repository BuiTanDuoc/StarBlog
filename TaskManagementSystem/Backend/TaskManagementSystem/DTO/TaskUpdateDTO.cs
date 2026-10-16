namespace TaskManagementSystem.DTO
{
    public record TaskUpdateDTO
    (
        string? Title,
        string? Description,
        string? Status,
        string? Priority,
        DateTime? DueDate,
        int? AssignedToUserId
    );
}
