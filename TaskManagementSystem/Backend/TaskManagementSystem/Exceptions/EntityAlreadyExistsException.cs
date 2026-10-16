namespace TaskManagementSystem.Exceptions
{
    /// <summary>
    /// Thrown when an entity already exists (e.g., duplicate email, duplicate username)
    /// HTTP 400 Bad Request
    /// </summary>
    public class EntityAlreadyExistsException : AppException
    {
        public EntityAlreadyExistsException(string entityName, string property)
            : base($"{entityName} with this {property} already exists.") { }

        public EntityAlreadyExistsException(string message) : base(message) { }
    }
}
