using AutoMapper.QueryableExtensions;
using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Features.Employees.DTOs;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Application.Features.Employees.Commands.UpdateEmployee;

public record UpdateEmployeeCommand : IRequest<Result<EmployeeDto>>
{
    public Guid Id { get; init; }
    public string FullName { get; init; } = default!;
    public string Position { get; init; } = default!;
    public decimal Salary { get; init; }
    public EmployeeStatus Status { get; init; }
    public Guid BranchId { get; init; }
    public Guid? DepartmentId { get; init; }
}

public class UpdateEmployeeCommandValidator : AbstractValidator<UpdateEmployeeCommand>
{
    public UpdateEmployeeCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Position).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Salary).GreaterThanOrEqualTo(0);
        RuleFor(x => x.BranchId).NotEmpty();
    }
}

public class UpdateEmployeeCommandHandler : IRequestHandler<UpdateEmployeeCommand, Result<EmployeeDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly AutoMapper.IMapper _mapper;

    public UpdateEmployeeCommandHandler(IUnitOfWork unitOfWork, AutoMapper.IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<EmployeeDto>> Handle(UpdateEmployeeCommand request, CancellationToken cancellationToken)
    {
        var repository = _unitOfWork.Repository<Employee>();
        var employee = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (employee is null)
        {
            return Result<EmployeeDto>.Failure($"Employee with id '{request.Id}' was not found.");
        }

        employee.FullName = request.FullName;
        employee.Position = request.Position;
        employee.Salary = request.Salary;
        employee.Status = request.Status;
        employee.BranchId = request.BranchId;
        employee.DepartmentId = request.DepartmentId;

        repository.Update(employee);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var dto = await repository.Query()
            .Where(e => e.Id == employee.Id)
            .ProjectTo<EmployeeDto>(_mapper.ConfigurationProvider)
            .FirstAsync(cancellationToken);

        return Result<EmployeeDto>.Success(dto);
    }
}
