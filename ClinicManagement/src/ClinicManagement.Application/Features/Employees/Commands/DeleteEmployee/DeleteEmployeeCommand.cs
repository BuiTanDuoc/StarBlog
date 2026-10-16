using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Entities;
using FluentValidation;
using MediatR;

namespace ClinicManagement.Application.Features.Employees.Commands.DeleteEmployee;

public record DeleteEmployeeCommand(Guid Id) : IRequest<Result>;

public class DeleteEmployeeCommandValidator : AbstractValidator<DeleteEmployeeCommand>
{
    public DeleteEmployeeCommandValidator() => RuleFor(x => x.Id).NotEmpty();
}

public class DeleteEmployeeCommandHandler : IRequestHandler<DeleteEmployeeCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteEmployeeCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Result> Handle(DeleteEmployeeCommand request, CancellationToken cancellationToken)
    {
        var repository = _unitOfWork.Repository<Employee>();
        var employee = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (employee is null)
        {
            return Result.Failure($"Employee with id '{request.Id}' was not found.");
        }

        repository.Remove(employee);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
