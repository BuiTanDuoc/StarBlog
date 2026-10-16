using AutoMapper;
using TaskManagementSystem.Repositories.Interfaces;
using TaskManagementSystem.Services.Interfaces;
namespace TaskManagementSystem.Services
{
    public class ApplicationService : IApplicationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILoggerFactory _loggerFactory;
        public ApplicationService(IUnitOfWork unitOfWork, IMapper mapper, ILoggerFactory loggerFactory)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _loggerFactory = loggerFactory;
        }


        public UserService UserService => new(_unitOfWork, _mapper, _loggerFactory.CreateLogger<UserService>());
        public ProjectService ProjectService => new(_unitOfWork, _mapper, _loggerFactory.CreateLogger<ProjectService>());
        public TaskService TaskService => new(_unitOfWork, _mapper, _loggerFactory.CreateLogger<TaskService>());
    }
}