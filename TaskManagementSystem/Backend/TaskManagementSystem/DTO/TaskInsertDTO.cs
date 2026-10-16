namespace TaskManagementSystem.DTO
{
    public record TaskInsertDTO
    (
        string Title,
        string? Description,
        string Status,
        string Priority,
        DateTime? DueDate,
        int? AssignedToUserId
    );
}
