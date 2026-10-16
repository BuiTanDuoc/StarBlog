using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Entities;
using FluentValidation;
using MediatR;

namespace ClinicManagement.Application.Features.Departments.Commands.DeleteDepartment;

public record DeleteDepartmentCommand(Guid Id) : IRequest<Result>;

public class DeleteDepartmentCommandValidator : AbstractValidator<DeleteDepartmentCommand>
{
    public DeleteDepartmentCommandValidator() => RuleFor(x => x.Id).NotEmpty();
}

public class DeleteDepartmentCommandHandler : IRequestHandler<DeleteDepartmentCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteDepartmentCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Result> Handle(DeleteDepartmentCommand request, CancellationToken cancellationToken)
    {
        var repository = _unitOfWork.Repository<Department>();
        var department = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (department is null)
        {
            return Result.Failure($"Department with id '{request.Id}' was not found.");
        }

        repository.Remove(department);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
