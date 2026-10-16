namespace TaskManagementSystem.Exceptions
{
    /// <summary>
    /// Thrown when an invalid operation is attempted
    /// HTTP 400 Bad Request
    /// </summary>
    public class InvalidOperationException : AppException
    {
        public InvalidOperationException(string message) : base(message) { }
    }
}
