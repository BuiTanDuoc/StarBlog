using AutoMapper.QueryableExtensions;
using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Services;
using ClinicManagement.Application.Features.Employees.DTOs;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Application.Features.Employees.Commands.CreateEmployee;

public record CreateEmployeeCommand : IRequest<Result<EmployeeDto>>
{
    public string FullName { get; init; } = default!;
    public string Position { get; init; } = default!;
    public DateTime HireDate { get; init; }
    public decimal Salary { get; init; }
    public Guid BranchId { get; init; }
    public Guid? DepartmentId { get; init; }
    public Guid? ApplicationUserId { get; init; }
}

public class CreateEmployeeCommandValidator : AbstractValidator<CreateEmployeeCommand>
{
    public CreateEmployeeCommandValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Position).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Salary).GreaterThanOrEqualTo(0);
        RuleFor(x => x.BranchId).NotEmpty();
    }
}

public class CreateEmployeeCommandHandler : IRequestHandler<CreateEmployeeCommand, Result<EmployeeDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ISubscriptionLimitService _subscriptionLimitService;
    private readonly AutoMapper.IMapper _mapper;

    public CreateEmployeeCommandHandler(
        IUnitOfWork unitOfWork, ICurrentUserService currentUserService,
        ISubscriptionLimitService subscriptionLimitService, AutoMapper.IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _subscriptionLimitService = subscriptionLimitService;
        _mapper = mapper;
    }

    public async Task<Result<EmployeeDto>> Handle(CreateEmployeeCommand request, CancellationToken cancellationToken)
    {
        await _subscriptionLimitService.EnsureCanAddEmployeeAsync(cancellationToken);

        var employee = new Employee
        {
            TenantId = _currentUserService.TenantId,
            FullName = request.FullName,
            Position = request.Position,
            HireDate = request.HireDate,
            Salary = request.Salary,
            BranchId = request.BranchId,
            DepartmentId = request.DepartmentId,
            ApplicationUserId = request.ApplicationUserId,
            Status = Domain.Enums.EmployeeStatus.Active
        };

        await _unitOfWork.Repository<Employee>().AddAsync(employee, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var dto = await _unitOfWork.Repository<Employee>().Query()
            .Where(e => e.Id == employee.Id)
            .ProjectTo<EmployeeDto>(_mapper.ConfigurationProvider)
            .FirstAsync(cancellationToken);

        return Result<EmployeeDto>.Success(dto);
    }
}
