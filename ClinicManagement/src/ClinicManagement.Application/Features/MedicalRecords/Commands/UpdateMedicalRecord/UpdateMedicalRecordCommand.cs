using AutoMapper.QueryableExtensions;
using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Features.MedicalRecords.DTOs;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Application.Features.MedicalRecords.Commands.UpdateMedicalRecord;

public record UpdateMedicalRecordCommand : IRequest<Result<MedicalRecordDto>>
{
    public Guid Id { get; init; }
    public string Diagnosis { get; init; } = default!;
    public string? Symptoms { get; init; }
    public string? TreatmentPlan { get; init; }
    public string? Notes { get; init; }
    public DateTime? FollowUpDate { get; init; }
    public Guid? TemplateId { get; init; }
    public string? CustomFieldsJson { get; init; }
}

public class UpdateMedicalRecordCommandValidator : AbstractValidator<UpdateMedicalRecordCommand>
{
    public UpdateMedicalRecordCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Diagnosis).NotEmpty().MaximumLength(1000);
    }
}

public class UpdateMedicalRecordCommandHandler : IRequestHandler<UpdateMedicalRecordCommand, Result<MedicalRecordDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly AutoMapper.IMapper _mapper;

    public UpdateMedicalRecordCommandHandler(IUnitOfWork unitOfWork, AutoMapper.IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<MedicalRecordDto>> Handle(UpdateMedicalRecordCommand request, CancellationToken cancellationToken)
    {
        var repository = _unitOfWork.Repository<MedicalRecord>();
        var record = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (record is null)
        {
            return Result<MedicalRecordDto>.Failure($"Medical record with id '{request.Id}' was not found.");
        }

        if (request.TemplateId is { } templateId)
        {
            var templateExists = await _unitOfWork.Repository<MedicalRecordTemplate>().Query()
                .AnyAsync(t => t.Id == templateId && t.IsActive, cancellationToken);
            if (!templateExists)
            {
                return Result<MedicalRecordDto>.Failure($"Template with id '{templateId}' was not found or is inactive.");
            }
        }

        record.Diagnosis = request.Diagnosis;
        record.Symptoms = request.Symptoms;
        record.TreatmentPlan = request.TreatmentPlan;
        record.Notes = request.Notes;
        record.FollowUpDate = request.FollowUpDate;
        record.TemplateId = request.TemplateId;
        record.CustomFieldsJson = request.CustomFieldsJson;

        repository.Update(record);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var dto = await repository.Query()
            .Where(r => r.Id == record.Id)
            .ProjectTo<MedicalRecordDto>(_mapper.ConfigurationProvider)
            .FirstAsync(cancellationToken);

        return Result<MedicalRecordDto>.Success(dto);
    }
}
