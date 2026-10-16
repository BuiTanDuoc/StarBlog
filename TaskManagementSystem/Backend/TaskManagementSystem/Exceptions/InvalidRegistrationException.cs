namespace TaskManagementSystem.Exceptions
{
    /// <summary>
    /// Thrown when a user attempts to register with invalid data
    /// HTTP 400 Bad Request
    /// </summary>
    public class InvalidRegistrationException : AppException
    {
        public InvalidRegistrationException(string message) : base(message) { }
    }
}
