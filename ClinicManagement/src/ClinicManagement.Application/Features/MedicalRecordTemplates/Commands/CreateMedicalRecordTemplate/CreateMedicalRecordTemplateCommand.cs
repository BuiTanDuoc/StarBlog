using System.Text.Json;
using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Features.MedicalRecordTemplates.DTOs;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Entities;
using FluentValidation;
using MediatR;

namespace ClinicManagement.Application.Features.MedicalRecordTemplates.Commands.CreateMedicalRecordTemplate;

public record CreateMedicalRecordTemplateCommand(string Name, string Specialization, string FieldsSchemaJson)
    : IRequest<Result<MedicalRecordTemplateDto>>;

public class CreateMedicalRecordTemplateCommandValidator : AbstractValidator<CreateMedicalRecordTemplateCommand>
{
    public CreateMedicalRecordTemplateCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Specialization).NotEmpty().MaximumLength(150);
        RuleFor(x => x.FieldsSchemaJson).NotEmpty()
            .Must(BeValidJsonArray).WithMessage("FieldsSchemaJson must be a valid JSON array of field definitions.");
    }

    // Only checks "is this a JSON array at all", not that each element has the
    // key/label/type shape the frontend's dynamic form renderer expects — a
    // lightweight guard against garbage input, not full schema validation.
    private static bool BeValidJsonArray(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            return doc.RootElement.ValueKind == JsonValueKind.Array;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}

public class CreateMedicalRecordTemplateCommandHandler
    : IRequestHandler<CreateMedicalRecordTemplateCommand, Result<MedicalRecordTemplateDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly AutoMapper.IMapper _mapper;

    public CreateMedicalRecordTemplateCommandHandler(
        IUnitOfWork unitOfWork, ICurrentUserService currentUserService, AutoMapper.IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _mapper = mapper;
    }

    public async Task<Result<MedicalRecordTemplateDto>> Handle(
        CreateMedicalRecordTemplateCommand request, CancellationToken cancellationToken)
    {
        var template = new MedicalRecordTemplate
        {
            TenantId = _currentUserService.TenantId,
            Name = request.Name,
            Specialization = request.Specialization,
            FieldsSchemaJson = request.FieldsSchemaJson,
            IsActive = true
        };

        await _unitOfWork.Repository<MedicalRecordTemplate>().AddAsync(template, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<MedicalRecordTemplateDto>.Success(_mapper.Map<MedicalRecordTemplateDto>(template));
    }
}
