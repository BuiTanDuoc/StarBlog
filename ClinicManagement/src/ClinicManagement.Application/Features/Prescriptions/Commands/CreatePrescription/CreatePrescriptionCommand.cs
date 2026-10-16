using AutoMapper.QueryableExtensions;
using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Features.Prescriptions.DTOs;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Application.Features.Prescriptions.Commands.CreatePrescription;

public record CreatePrescriptionItemDto(string MedicineName, string Dosage, string Frequency, string Duration, string? Instructions);

public record CreatePrescriptionCommand : IRequest<Result<PrescriptionDto>>
{
    public Guid MedicalRecordId { get; init; }
    public Guid PatientId { get; init; }
    public Guid DoctorId { get; init; }
    public List<CreatePrescriptionItemDto> Items { get; init; } = new();
}

public class CreatePrescriptionCommandValidator : AbstractValidator<CreatePrescriptionCommand>
{
    public CreatePrescriptionCommandValidator()
    {
        RuleFor(x => x.MedicalRecordId).NotEmpty();
        RuleFor(x => x.PatientId).NotEmpty();
        RuleFor(x => x.DoctorId).NotEmpty();
        RuleFor(x => x.Items).NotEmpty().WithMessage("A prescription must contain at least one medicine.");
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.MedicineName).NotEmpty().MaximumLength(200);
            item.RuleFor(i => i.Dosage).NotEmpty().MaximumLength(100);
            item.RuleFor(i => i.Frequency).NotEmpty().MaximumLength(100);
            item.RuleFor(i => i.Duration).NotEmpty().MaximumLength(100);
        });
    }
}

public class CreatePrescriptionCommandHandler : IRequestHandler<CreatePrescriptionCommand, Result<PrescriptionDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDrugSafetyChecker _drugSafetyChecker;
    private readonly AutoMapper.IMapper _mapper;

    public CreatePrescriptionCommandHandler(
        IUnitOfWork unitOfWork, ICurrentUserService currentUserService, IDrugSafetyChecker drugSafetyChecker, AutoMapper.IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _drugSafetyChecker = drugSafetyChecker;
        _mapper = mapper;
    }

    public async Task<Result<PrescriptionDto>> Handle(CreatePrescriptionCommand request, CancellationToken cancellationToken)
    {
        var prescription = new Prescription
        {
            TenantId = _currentUserService.TenantId,
            MedicalRecordId = request.MedicalRecordId,
            PatientId = request.PatientId,
            DoctorId = request.DoctorId,
            DateIssued = DateTime.UtcNow
        };

        foreach (var item in request.Items)
        {
            prescription.Items.Add(new PrescriptionItem
            {
                TenantId = _currentUserService.TenantId,
                MedicineName = item.MedicineName,
                Dosage = item.Dosage,
                Frequency = item.Frequency,
                Duration = item.Duration,
                Instructions = item.Instructions
            });
        }

        await _unitOfWork.Repository<Prescription>().AddAsync(prescription, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var dto = await _unitOfWork.Repository<Prescription>().Query()
            .Where(p => p.Id == prescription.Id)
            .ProjectTo<PrescriptionDto>(_mapper.ConfigurationProvider)
            .FirstAsync(cancellationToken);

        // Checked against this prescription's own items plus the patient's other
        // currently-active prescriptions (last 90 days is a simple, conservative proxy for
        // "still relevant" without a formal medication end-date to check against) — a new
        // item can interact with something prescribed last week just as easily as with
        // another item on the same script.
        var recentMedicineNames = await _unitOfWork.Repository<Prescription>().Query()
            .Where(p => p.PatientId == request.PatientId && p.DateIssued >= DateTime.UtcNow.AddDays(-90))
            .SelectMany(p => p.Items.Select(i => i.MedicineName))
            .ToListAsync(cancellationToken);

        var allergies = await _unitOfWork.Repository<Patient>().Query()
            .Where(p => p.Id == request.PatientId)
            .Select(p => p.Allergies)
            .FirstOrDefaultAsync(cancellationToken);

        var allMedicineNames = request.Items.Select(i => i.MedicineName).Union(recentMedicineNames);
        dto.Warnings = _drugSafetyChecker.Check(allergies, allMedicineNames)
            .Select(w => $"[{w.Severity}] {w.Message}")
            .ToList();

        return Result<PrescriptionDto>.Success(dto);
    }
}
