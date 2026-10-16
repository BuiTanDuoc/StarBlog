using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Entities;
using FluentValidation;
using MediatR;

namespace ClinicManagement.Application.Features.TreatmentPackages.Commands.DeleteTreatmentPackage;

public record DeleteTreatmentPackageCommand(Guid Id) : IRequest<Result>;

public class DeleteTreatmentPackageCommandValidator : AbstractValidator<DeleteTreatmentPackageCommand>
{
    public DeleteTreatmentPackageCommandValidator() => RuleFor(x => x.Id).NotEmpty();
}

public class DeleteTreatmentPackageCommandHandler : IRequestHandler<DeleteTreatmentPackageCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteTreatmentPackageCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Result> Handle(DeleteTreatmentPackageCommand request, CancellationToken cancellationToken)
    {
        var repository = _unitOfWork.Repository<TreatmentPackage>();
        var package = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (package is null)
        {
            return Result.Failure($"Package with id '{request.Id}' was not found.");
        }

        // Soft delete — patients who already bought this package keep their PatientPackage
        // row (it doesn't cascade), this only stops it being offered for new sales.
        repository.Remove(package);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
