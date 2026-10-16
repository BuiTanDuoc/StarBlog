namespace TaskManagementSystem.Exceptions
{
    /// <summary>
    /// Thrown when an entity is not found
    /// HTTP 404 Not Found
    /// </summary>
    public class EntityNotFoundException : AppException
    {
        public EntityNotFoundException(string entityName, int id)
            : base($"{entityName} with ID {id} not found.") { }

        public EntityNotFoundException(string message, string v) : base(message) { }
    }
}
