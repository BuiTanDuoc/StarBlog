namespace TaskManagementSystem.Services.Interfaces
{
    public interface IApplicationService
    {

        UserService UserService { get; }
        ProjectService ProjectService { get; }
        TaskService TaskService { get; }
    }
}