using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Features.Plans.DTOs;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Entities;
using FluentValidation;
using MediatR;

namespace ClinicManagement.Application.Features.Plans.Commands.UpdatePlan;

public record UpdatePlanCommand : IRequest<Result<PlanDto>>
{
    public Guid Id { get; init; }
    public string Name { get; init; } = default!;
    public decimal Price { get; init; }
    public int DurationInDays { get; init; }
    public int MaxDoctors { get; init; }
    public int MaxEmployees { get; init; }
    public int MaxPatients { get; init; }
    public int MaxAppointmentsPerMonth { get; init; }
    public long StorageLimitMb { get; init; }
    public bool IsActive { get; init; }
}

public class UpdatePlanCommandValidator : AbstractValidator<UpdatePlanCommand>
{
    public UpdatePlanCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0);
        RuleFor(x => x.DurationInDays).GreaterThan(0);
    }
}

public class UpdatePlanCommandHandler : IRequestHandler<UpdatePlanCommand, Result<PlanDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly AutoMapper.IMapper _mapper;

    public UpdatePlanCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService, AutoMapper.IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _mapper = mapper;
    }

    public async Task<Result<PlanDto>> Handle(UpdatePlanCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsSuperAdmin)
        {
            return Result<PlanDto>.Failure("Only platform administrators can manage plans.");
        }

        var repository = _unitOfWork.Repository<Plan>();
        var plan = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (plan is null)
        {
            return Result<PlanDto>.Failure($"Plan with id '{request.Id}' was not found.");
        }

        // Note: shrinking limits below a tenant's current usage is intentionally allowed here —
        // it doesn't retroactively break existing records, it just blocks new ones via
        // ISubscriptionLimitService until the tenant is back under the new cap.
        plan.Name = request.Name;
        plan.Price = request.Price;
        plan.DurationInDays = request.DurationInDays;
        plan.MaxDoctors = request.MaxDoctors;
        plan.MaxEmployees = request.MaxEmployees;
        plan.MaxPatients = request.MaxPatients;
        plan.MaxAppointmentsPerMonth = request.MaxAppointmentsPerMonth;
        plan.StorageLimitMb = request.StorageLimitMb;
        plan.IsActive = request.IsActive;

        repository.Update(plan);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<PlanDto>.Success(_mapper.Map<PlanDto>(plan));
    }
}
