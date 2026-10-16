using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Features.Plans.DTOs;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;
using FluentValidation;
using MediatR;

namespace ClinicManagement.Application.Features.Plans.Commands.CreatePlan;

public record CreatePlanCommand : IRequest<Result<PlanDto>>
{
    public string Name { get; init; } = default!;
    public PlanTier Tier { get; init; }
    public decimal Price { get; init; }
    public int DurationInDays { get; init; } = 30;
    public int MaxDoctors { get; init; }
    public int MaxEmployees { get; init; }
    public int MaxPatients { get; init; }
    public int MaxAppointmentsPerMonth { get; init; }
    public long StorageLimitMb { get; init; }
}

public class CreatePlanCommandValidator : AbstractValidator<CreatePlanCommand>
{
    public CreatePlanCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0);
        RuleFor(x => x.DurationInDays).GreaterThan(0);
        RuleFor(x => x.MaxDoctors).GreaterThanOrEqualTo(0);
        RuleFor(x => x.MaxEmployees).GreaterThanOrEqualTo(0);
        RuleFor(x => x.MaxPatients).GreaterThanOrEqualTo(0);
        RuleFor(x => x.MaxAppointmentsPerMonth).GreaterThanOrEqualTo(0);
        RuleFor(x => x.StorageLimitMb).GreaterThanOrEqualTo(0);
    }
}

public class CreatePlanCommandHandler : IRequestHandler<CreatePlanCommand, Result<PlanDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly AutoMapper.IMapper _mapper;

    public CreatePlanCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService, AutoMapper.IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _mapper = mapper;
    }

    public async Task<Result<PlanDto>> Handle(CreatePlanCommand request, CancellationToken cancellationToken)
    {
        // [HasPermission(Permissions.Tenants.Manage)] guards this at the controller (Phase 6);
        // re-checked here too since Plans are a platform-global resource, not tenant-scoped,
        // so a misconfigured route wouldn't otherwise be caught by the tenant query filter.
        if (!_currentUserService.IsSuperAdmin)
        {
            return Result<PlanDto>.Failure("Only platform administrators can manage plans.");
        }

        var plan = new Plan
        {
            Name = request.Name,
            Tier = request.Tier,
            Price = request.Price,
            DurationInDays = request.DurationInDays,
            MaxDoctors = request.MaxDoctors,
            MaxEmployees = request.MaxEmployees,
            MaxPatients = request.MaxPatients,
            MaxAppointmentsPerMonth = request.MaxAppointmentsPerMonth,
            StorageLimitMb = request.StorageLimitMb,
            IsActive = true
        };

        await _unitOfWork.Repository<Plan>().AddAsync(plan, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<PlanDto>.Success(_mapper.Map<PlanDto>(plan));
    }
}
