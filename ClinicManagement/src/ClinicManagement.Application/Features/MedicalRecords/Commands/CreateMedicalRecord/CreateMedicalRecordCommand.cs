using AutoMapper.QueryableExtensions;
using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Features.MedicalRecords.DTOs;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Application.Features.MedicalRecords.Commands.CreateMedicalRecord;

public record CreateMedicalRecordCommand : IRequest<Result<MedicalRecordDto>>
{
    public Guid PatientId { get; init; }
    public Guid DoctorId { get; init; }
    public Guid? AppointmentId { get; init; }
    public string Diagnosis { get; init; } = default!;
    public string? Symptoms { get; init; }
    public string? TreatmentPlan { get; init; }
    public string? Notes { get; init; }
    public DateTime? FollowUpDate { get; init; }

    /// <summary>Optional — pick a MedicalRecordTemplate (e.g. "Dental Exam") to fill in specialty-specific fields alongside the generic Diagnosis/Symptoms/Notes above.</summary>
    public Guid? TemplateId { get; init; }

    /// <summary>Raw JSON object of {fieldKey: value}, expected to match the chosen Template's FieldsSchemaJson keys. Not validated field-by-field against the schema server-side yet — see the note on CreateMedicalRecordCommandHandler.</summary>
    public string? CustomFieldsJson { get; init; }
}

public class CreateMedicalRecordCommandValidator : AbstractValidator<CreateMedicalRecordCommand>
{
    public CreateMedicalRecordCommandValidator()
    {
        RuleFor(x => x.PatientId).NotEmpty();
        RuleFor(x => x.DoctorId).NotEmpty();
        RuleFor(x => x.Diagnosis).NotEmpty().MaximumLength(1000);
    }
}

public class CreateMedicalRecordCommandHandler : IRequestHandler<CreateMedicalRecordCommand, Result<MedicalRecordDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly AutoMapper.IMapper _mapper;

    public CreateMedicalRecordCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService, AutoMapper.IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _mapper = mapper;
    }

    public async Task<Result<MedicalRecordDto>> Handle(CreateMedicalRecordCommand request, CancellationToken cancellationToken)
    {
        if (request.TemplateId is { } templateId)
        {
            var templateExists = await _unitOfWork.Repository<MedicalRecordTemplate>().Query()
                .AnyAsync(t => t.Id == templateId && t.IsActive, cancellationToken);
            if (!templateExists)
            {
                return Result<MedicalRecordDto>.Failure($"Template with id '{templateId}' was not found or is inactive.");
            }
        }

        var record = new MedicalRecord
        {
            TenantId = _currentUserService.TenantId,
            PatientId = request.PatientId,
            DoctorId = request.DoctorId,
            AppointmentId = request.AppointmentId,
            Diagnosis = request.Diagnosis,
            Symptoms = request.Symptoms,
            TreatmentPlan = request.TreatmentPlan,
            Notes = request.Notes,
            FollowUpDate = request.FollowUpDate,
            TemplateId = request.TemplateId,
            // Not schema-validated against Template.FieldsSchemaJson field-by-field here —
            // this trusts the frontend's dynamic form to have submitted keys matching the
            // chosen template. A follow-up worth doing before relying on this for anything
            // clinically load-bearing: validate CustomFieldsJson's keys against the
            // template's schema server-side, the same way FluentValidation validates
            // everything else in this command.
            CustomFieldsJson = request.CustomFieldsJson
        };

        await _unitOfWork.Repository<MedicalRecord>().AddAsync(record, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var dto = await _unitOfWork.Repository<MedicalRecord>().Query()
            .Where(r => r.Id == record.Id)
            .ProjectTo<MedicalRecordDto>(_mapper.ConfigurationProvider)
            .FirstAsync(cancellationToken);

        return Result<MedicalRecordDto>.Success(dto);
    }
}
