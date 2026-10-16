using AutoMapper;
using TaskManagementSystem.Data;
using TaskManagementSystem.DTO;

public class MapperConfig : Profile
{
    public MapperConfig()
    {
        // ============= USER MAPPINGS =============
        CreateMap<User, UserReadOnlyDTO>().ReverseMap();

        // ✅ DELETE the explicit list mapping - AutoMapper handles it automatically!

        CreateMap<UserRegisterDTO, User>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Password, opt => opt.Ignore())
            .ForMember(dest => dest.UserRole, opt => opt.Ignore());

        CreateMap<UpdateUserDTO, User>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

        // ============= PROJECT MAPPINGS =============
        CreateMap<ProjectInsertDTO, Project>();

        CreateMap<ProjectUpdateDTO, Project>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

        CreateMap<Project, ProjectReadOnlyDTO>()
            .ForMember(dest => dest.OwnerName, opt => opt.MapFrom(src => src.Owner.Username))
            .ForMember(dest => dest.MemberCount, opt => opt.MapFrom(src =>
                src.Members.Count(m => !m.isDeleted)))
            .ForMember(dest => dest.TaskCount, opt => opt.MapFrom(src =>
                src.Tasks.Count(t => !t.isDeleted)))
            .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => src.InsertedAt));

        // ============= PROJECT MEMBER MAPPINGS =============
        CreateMap<ProjectMember, ProjectMemberReadOnlyDTO>()
            .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.User.Username))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User.Email));

        // ============= TASK MAPPINGS =============
        CreateMap<TaskInsertDTO, ProjectTask>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src =>
                Enum.Parse<TaskManagementSystem.Core.Enums.TaskStatus>(
                    src.Status ?? "Todo", true)))
            .ForMember(dest => dest.Priority, opt => opt.MapFrom(src =>
                Enum.Parse<TaskManagementSystem.Core.Enums.TaskPriority>(
                    src.Priority ?? "Medium", true)));

        CreateMap<TaskUpdateDTO, ProjectTask>()
            .ForMember(dest => dest.Status, opt =>
            {
                opt.Condition(src => !string.IsNullOrWhiteSpace(src.Status));
                opt.MapFrom(src => Enum.Parse<TaskManagementSystem.Core.Enums.TaskStatus>(src.Status, true));
            })
            .ForMember(dest => dest.Priority, opt =>
            {
                opt.Condition(src => !string.IsNullOrWhiteSpace(src.Priority));
                opt.MapFrom(src => Enum.Parse<TaskManagementSystem.Core.Enums.TaskPriority>(src.Priority, true));
            })
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

        CreateMap<ProjectTask, TaskReadOnlyDTO>()
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.Priority, opt => opt.MapFrom(src => src.Priority.ToString()))
            .ForMember(dest => dest.AssignedToUserId, opt => opt.MapFrom(src => src.AssignedToUserId))
            .ForMember(dest => dest.AssignedToUsername, opt => opt.MapFrom(src =>
                src.AssignedToUser != null ? src.AssignedToUser.Username : null))
            .ForMember(dest => dest.Comments, opt => opt.MapFrom(src =>
                src.Comments.Where(c => !c.isDeleted).ToList()))
            .ForMember(dest => dest.CommentCount, opt => opt.MapFrom(src =>
                src.Comments.Count(c => !c.isDeleted)))
            .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => src.InsertedAt))
            .ForMember(dest => dest.DueDate, opt => opt.MapFrom(src => src.DueDate))
            .ForMember(dest => dest.ProjectId, opt => opt.MapFrom(src => src.ProjectId));

        // ============= TASK COMMENT MAPPINGS =============
        CreateMap<TaskCommentInsertDTO, TaskComment>();

        CreateMap<TaskComment, TaskCommentReadOnlyDTO>()
            .ForMember(dest => dest.Username, opt => opt.MapFrom(src =>
                src.User != null ? src.User.Username : "Unknown User"))
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
            .ForMember(dest => dest.Content, opt => opt.MapFrom(src => src.Content))
            .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => src.InsertedAt));
    }
}