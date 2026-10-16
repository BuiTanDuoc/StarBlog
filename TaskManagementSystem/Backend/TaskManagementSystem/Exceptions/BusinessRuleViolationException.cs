namespace TaskManagementSystem.Exceptions
{
    /// <summary>
    /// Thrown when trying to perform an action that violates business rules
    /// HTTP 400 Bad Request
    /// </summary>
    public class BusinessRuleViolationException : AppException
    {
        public BusinessRuleViolationException(string message) : base(message) { }
    }
}