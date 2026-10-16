using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Services;
using ClinicManagement.Application.Features.Patients.DTOs;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Application.Features.Patients.Commands.CreatePatient;

public record CreatePatientCommand : IRequest<Result<PatientDto>>
{
    public string? NationalId { get; init; }
    public string FullName { get; init; } = default!;
    public Gender Gender { get; init; }
    public DateTime DateOfBirth { get; init; }
    public string Phone { get; init; } = default!;
    public string? Address { get; init; }
    public BloodGroup BloodGroup { get; init; } = BloodGroup.Unknown;
    public string? EmergencyContactName { get; init; }
    public string? EmergencyContactPhone { get; init; }
    public string? Allergies { get; init; }
    public string? MedicalHistory { get; init; }
    public string? InsuranceProvider { get; init; }
    public string? InsuranceNumber { get; init; }
}

public class CreatePatientCommandValidator : AbstractValidator<CreatePatientCommand>
{
    public CreatePatientCommandValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Phone).NotEmpty().MaximumLength(30);
        RuleFor(x => x.DateOfBirth).NotEmpty().LessThan(DateTime.UtcNow).WithMessage("Date of birth must be in the past.");
        RuleFor(x => x.NationalId).MaximumLength(50);
        RuleFor(x => x.EmergencyContactPhone).MaximumLength(30);
    }
}

public class CreatePatientCommandHandler : IRequestHandler<CreatePatientCommand, Result<PatientDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ISubscriptionLimitService _subscriptionLimitService;
    private readonly AutoMapper.IMapper _mapper;

    public CreatePatientCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        ISubscriptionLimitService subscriptionLimitService,
        AutoMapper.IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _subscriptionLimitService = subscriptionLimitService;
        _mapper = mapper;
    }

    public async Task<Result<PatientDto>> Handle(CreatePatientCommand request, CancellationToken cancellationToken)
    {
        await _subscriptionLimitService.EnsureCanAddPatientAsync(cancellationToken);

        var patient = new Patient
        {
            TenantId = _currentUserService.TenantId,
            PatientCode = await GenerateNextPatientCodeAsync(cancellationToken),
            NationalId = request.NationalId,
            FullName = request.FullName,
            Gender = request.Gender,
            DateOfBirth = request.DateOfBirth,
            Phone = request.Phone,
            Address = request.Address,
            BloodGroup = request.BloodGroup,
            EmergencyContactName = request.EmergencyContactName,
            EmergencyContactPhone = request.EmergencyContactPhone,
            Allergies = request.Allergies,
            MedicalHistory = request.MedicalHistory,
            InsuranceProvider = request.InsuranceProvider,
            InsuranceNumber = request.InsuranceNumber
        };

        await _unitOfWork.Repository<Patient>().AddAsync(patient, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<PatientDto>.Success(_mapper.Map<PatientDto>(patient));
    }

    /// <summary>
    /// Generates "PT-000001" style codes per tenant. Reads the current max
    /// under normal (non-serializable) isolation — a race between two
    /// simultaneous creates could in theory produce a duplicate, which the
    /// unique index (TenantId, PatientCode) then rejects, surfacing as a
    /// retryable conflict. Acceptable trade-off for a human-facing code that
    /// doesn't need to be gap-free.
    /// </summary>
    private async Task<string> GenerateNextPatientCodeAsync(CancellationToken cancellationToken)
    {
        var count = await _unitOfWork.Repository<Patient>().Query()
            .IgnoreQueryFilters()
            .Where(p => p.TenantId == _currentUserService.TenantId)
            .CountAsync(cancellationToken);

        return $"PT-{(count + 1):D6}";
    }
}
