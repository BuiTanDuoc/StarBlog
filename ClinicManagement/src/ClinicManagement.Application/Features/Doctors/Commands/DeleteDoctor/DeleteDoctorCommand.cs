using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Entities;
using FluentValidation;
using MediatR;

namespace ClinicManagement.Application.Features.Doctors.Commands.DeleteDoctor;

public record DeleteDoctorCommand(Guid Id) : IRequest<Result>;

public class DeleteDoctorCommandValidator : AbstractValidator<DeleteDoctorCommand>
{
    public DeleteDoctorCommandValidator() => RuleFor(x => x.Id).NotEmpty();
}

public class DeleteDoctorCommandHandler : IRequestHandler<DeleteDoctorCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteDoctorCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Result> Handle(DeleteDoctorCommand request, CancellationToken cancellationToken)
    {
        var repository = _unitOfWork.Repository<Doctor>();
        var doctor = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (doctor is null)
        {
            return Result.Failure($"Doctor with id '{request.Id}' was not found.");
        }

        repository.Remove(doctor);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
