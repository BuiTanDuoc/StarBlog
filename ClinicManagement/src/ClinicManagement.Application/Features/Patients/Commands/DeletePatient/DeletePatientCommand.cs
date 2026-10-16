using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Entities;
using FluentValidation;
using MediatR;

namespace ClinicManagement.Application.Features.Patients.Commands.DeletePatient;

public record DeletePatientCommand(Guid Id) : IRequest<Result>;

public class DeletePatientCommandValidator : AbstractValidator<DeletePatientCommand>
{
    public DeletePatientCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}

public class DeletePatientCommandHandler : IRequestHandler<DeletePatientCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeletePatientCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(DeletePatientCommand request, CancellationToken cancellationToken)
    {
        var repository = _unitOfWork.Repository<Patient>();
        var patient = await repository.GetByIdAsync(request.Id, cancellationToken);

        if (patient is null)
        {
            return Result.Failure($"Patient with id '{request.Id}' was not found.");
        }

        // Removed here becomes a soft delete (IsDeleted = true) courtesy of
        // AuditableEntitySaveChangesInterceptor — the row and its history
        // (appointments, invoices, medical records) are preserved.
        repository.Remove(patient);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
