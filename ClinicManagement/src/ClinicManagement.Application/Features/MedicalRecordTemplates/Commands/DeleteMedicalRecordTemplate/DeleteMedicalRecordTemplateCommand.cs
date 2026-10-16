using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Entities;
using FluentValidation;
using MediatR;

namespace ClinicManagement.Application.Features.MedicalRecordTemplates.Commands.DeleteMedicalRecordTemplate;

public record DeleteMedicalRecordTemplateCommand(Guid Id) : IRequest<Result>;

public class DeleteMedicalRecordTemplateCommandValidator : AbstractValidator<DeleteMedicalRecordTemplateCommand>
{
    public DeleteMedicalRecordTemplateCommandValidator() => RuleFor(x => x.Id).NotEmpty();
}

public class DeleteMedicalRecordTemplateCommandHandler : IRequestHandler<DeleteMedicalRecordTemplateCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteMedicalRecordTemplateCommandHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Result> Handle(DeleteMedicalRecordTemplateCommand request, CancellationToken cancellationToken)
    {
        var repository = _unitOfWork.Repository<MedicalRecordTemplate>();
        var template = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (template is null)
        {
            return Result.Failure($"Template with id '{request.Id}' was not found.");
        }

        // Soft delete (see AuditableEntitySaveChangesInterceptor) — existing MedicalRecords
        // keep their TemplateId (FK is SetNull only on a hard delete, which this isn't), so
        // historical records still show which template they were filled in against even
        // after a clinic retires that template.
        repository.Remove(template);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
