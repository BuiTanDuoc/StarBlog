namespace TaskManagementSystem.Exceptions
{
    /// <summary>
    /// Thrown when user is not authenticated (invalid/missing token)
    /// HTTP 401 Unauthorized
    /// </summary>
    public class EntityNotAuthorizedException : AppException
    {
        public EntityNotAuthorizedException(string message) : base(message) { }

        public EntityNotAuthorizedException(string entityName, string reason)
            : base($"User not authenticated: {reason}") { }
    }
}
