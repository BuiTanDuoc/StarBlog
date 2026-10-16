using AutoMapper.QueryableExtensions;
using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Mappings;
using ClinicManagement.Application.Features.Notifications.DTOs;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Shared.Common;
using MediatR;

namespace ClinicManagement.Application.Features.Notifications.Queries.GetMyNotifications;

public record GetMyNotificationsQuery : IRequest<PaginatedList<NotificationDto>>
{
    public bool UnreadOnly { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

public class GetMyNotificationsQueryHandler : IRequestHandler<GetMyNotificationsQuery, PaginatedList<NotificationDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly AutoMapper.IMapper _mapper;

    public GetMyNotificationsQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService, AutoMapper.IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _mapper = mapper;
    }

    public async Task<PaginatedList<NotificationDto>> Handle(GetMyNotificationsQuery request, CancellationToken cancellationToken)
    {
        var query = _unitOfWork.Repository<Notification>().Query()
            .Where(n => n.UserId == _currentUserService.UserId);

        if (request.UnreadOnly) query = query.Where(n => !n.IsRead);

        return await query
            .OrderByDescending(n => n.CreatedAt)
            .ProjectTo<NotificationDto>(_mapper.ConfigurationProvider)
            .ToPaginatedListAsync(request.PageNumber, request.PageSize);
    }
}
