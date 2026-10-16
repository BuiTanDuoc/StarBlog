using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Features.MedicalRecordTemplates.DTOs;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Entities;
using FluentValidation;
using MediatR;

namespace ClinicManagement.Application.Features.MedicalRecordTemplates.Commands.UpdateMedicalRecordTemplate;

public record UpdateMedicalRecordTemplateCommand(Guid Id, string Name, string Specialization, string FieldsSchemaJson, bool IsActive)
    : IRequest<Result<MedicalRecordTemplateDto>>;

public class UpdateMedicalRecordTemplateCommandValidator : AbstractValidator<UpdateMedicalRecordTemplateCommand>
{
    public UpdateMedicalRecordTemplateCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Specialization).NotEmpty().MaximumLength(150);
        RuleFor(x => x.FieldsSchemaJson).NotEmpty();
    }
}

public class UpdateMedicalRecordTemplateCommandHandler
    : IRequestHandler<UpdateMedicalRecordTemplateCommand, Result<MedicalRecordTemplateDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly AutoMapper.IMapper _mapper;

    public UpdateMedicalRecordTemplateCommandHandler(IUnitOfWork unitOfWork, AutoMapper.IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<MedicalRecordTemplateDto>> Handle(
        UpdateMedicalRecordTemplateCommand request, CancellationToken cancellationToken)
    {
        var repository = _unitOfWork.Repository<MedicalRecordTemplate>();
        var template = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (template is null)
        {
            return Result<MedicalRecordTemplateDto>.Failure($"Template with id '{request.Id}' was not found.");
        }

        template.Name = request.Name;
        template.Specialization = request.Specialization;
        template.FieldsSchemaJson = request.FieldsSchemaJson;
        template.IsActive = request.IsActive;

        repository.Update(template);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<MedicalRecordTemplateDto>.Success(_mapper.Map<MedicalRecordTemplateDto>(template));
    }
}
