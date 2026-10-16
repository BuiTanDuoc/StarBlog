using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Features.Patients.DTOs;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;
using FluentValidation;
using MediatR;

namespace ClinicManagement.Application.Features.Patients.Commands.UpdatePatient;

public record UpdatePatientCommand : IRequest<Result<PatientDto>>
{
    public Guid Id { get; init; }
    public string? NationalId { get; init; }
    public string FullName { get; init; } = default!;
    public Gender Gender { get; init; }
    public DateTime DateOfBirth { get; init; }
    public string Phone { get; init; } = default!;
    public string? Address { get; init; }
    public BloodGroup BloodGroup { get; init; }
    public string? EmergencyContactName { get; init; }
    public string? EmergencyContactPhone { get; init; }
    public string? Allergies { get; init; }
    public string? MedicalHistory { get; init; }
    public string? InsuranceProvider { get; init; }
    public string? InsuranceNumber { get; init; }
}

public class UpdatePatientCommandValidator : AbstractValidator<UpdatePatientCommand>
{
    public UpdatePatientCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Phone).NotEmpty().MaximumLength(30);
        RuleFor(x => x.DateOfBirth).NotEmpty().LessThan(DateTime.UtcNow);
    }
}

public class UpdatePatientCommandHandler : IRequestHandler<UpdatePatientCommand, Result<PatientDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly AutoMapper.IMapper _mapper;

    public UpdatePatientCommandHandler(IUnitOfWork unitOfWork, AutoMapper.IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<PatientDto>> Handle(UpdatePatientCommand request, CancellationToken cancellationToken)
    {
        var repository = _unitOfWork.Repository<Patient>();
        var patient = await repository.GetByIdAsync(request.Id, cancellationToken);

        if (patient is null)
        {
            return Result<PatientDto>.Failure($"Patient with id '{request.Id}' was not found.");
        }

        patient.NationalId = request.NationalId;
        patient.FullName = request.FullName;
        patient.Gender = request.Gender;
        patient.DateOfBirth = request.DateOfBirth;
        patient.Phone = request.Phone;
        patient.Address = request.Address;
        patient.BloodGroup = request.BloodGroup;
        patient.EmergencyContactName = request.EmergencyContactName;
        patient.EmergencyContactPhone = request.EmergencyContactPhone;
        patient.Allergies = request.Allergies;
        patient.MedicalHistory = request.MedicalHistory;
        patient.InsuranceProvider = request.InsuranceProvider;
        patient.InsuranceNumber = request.InsuranceNumber;

        repository.Update(patient);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<PatientDto>.Success(_mapper.Map<PatientDto>(patient));
    }
}
