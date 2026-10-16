namespace TaskManagementSystem.Exceptions
{
    /// <summary>
    /// Thrown when user is authenticated but doesn't have permission for the action
    /// HTTP 403 Forbidden
    /// </summary>
    public class EntityForbiddenException : AppException
    {
        public EntityForbiddenException(string message) : base(message) { }

        public EntityForbiddenException(string action, string reason)
            : base($"You do not have permission to {action}. {reason}") { }
    }
}
