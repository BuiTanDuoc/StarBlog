namespace TaskManagementSystem.Exceptions
{
    /// <summary>
    /// Base exception for all custom application exceptions
    /// </summary>
    public abstract class AppException : Exception
    {
        protected AppException(string message) : base(message) { }
    }
}
