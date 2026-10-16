using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Entities;
using FluentValidation;
using MediatR;

namespace ClinicManagement.Application.Features.Notifications.Commands.MarkNotificationAsRead;

public record MarkNotificationAsReadCommand(Guid Id) : IRequest<Result>;

public class MarkNotificationAsReadCommandValidator : AbstractValidator<MarkNotificationAsReadCommand>
{
    public MarkNotificationAsReadCommandValidator() => RuleFor(x => x.Id).NotEmpty();
}

public class MarkNotificationAsReadCommandHandler : IRequestHandler<MarkNotificationAsReadCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public MarkNotificationAsReadCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(MarkNotificationAsReadCommand request, CancellationToken cancellationToken)
    {
        var repository = _unitOfWork.Repository<Notification>();
        var notification = await repository.GetByIdAsync(request.Id, cancellationToken);

        // Tenant isolation already stops cross-tenant access; this additionally stops one
        // staff member from marking a colleague's notification as read within the same tenant,
        // since Notification.UserId — not just TenantId — scopes ownership here.
        if (notification is null || notification.UserId != _currentUserService.UserId)
        {
            return Result.Failure("Notification not found.");
        }

        if (!notification.IsRead)
        {
            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
            repository.Update(notification);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return Result.Success();
    }
}
