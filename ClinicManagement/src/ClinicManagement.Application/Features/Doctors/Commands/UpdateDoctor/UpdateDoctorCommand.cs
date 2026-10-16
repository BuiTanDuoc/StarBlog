using AutoMapper.QueryableExtensions;
using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Features.Doctors.DTOs;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Application.Features.Doctors.Commands.UpdateDoctor;

public record UpdateDoctorCommand : IRequest<Result<DoctorDto>>
{
    public Guid Id { get; init; }
    public string FullName { get; init; } = default!;
    public string Specialization { get; init; } = default!;
    public decimal ConsultationFee { get; init; }
    public string WorkScheduleJson { get; init; } = "[]";
    public int YearsOfExperience { get; init; }
    public DoctorStatus Status { get; init; }
    public Guid BranchId { get; init; }
    public Guid? DepartmentId { get; init; }
}

public class UpdateDoctorCommandValidator : AbstractValidator<UpdateDoctorCommand>
{
    public UpdateDoctorCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Specialization).NotEmpty().MaximumLength(150);
        RuleFor(x => x.ConsultationFee).GreaterThanOrEqualTo(0);
        RuleFor(x => x.BranchId).NotEmpty();
    }
}

public class UpdateDoctorCommandHandler : IRequestHandler<UpdateDoctorCommand, Result<DoctorDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly AutoMapper.IMapper _mapper;

    public UpdateDoctorCommandHandler(IUnitOfWork unitOfWork, AutoMapper.IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<DoctorDto>> Handle(UpdateDoctorCommand request, CancellationToken cancellationToken)
    {
        var repository = _unitOfWork.Repository<Doctor>();
        var doctor = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (doctor is null)
        {
            return Result<DoctorDto>.Failure($"Doctor with id '{request.Id}' was not found.");
        }

        doctor.FullName = request.FullName;
        doctor.Specialization = request.Specialization;
        doctor.ConsultationFee = request.ConsultationFee;
        doctor.WorkScheduleJson = request.WorkScheduleJson;
        doctor.YearsOfExperience = request.YearsOfExperience;
        doctor.Status = request.Status;
        doctor.BranchId = request.BranchId;
        doctor.DepartmentId = request.DepartmentId;

        repository.Update(doctor);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var dto = await repository.Query()
            .Where(d => d.Id == doctor.Id)
            .ProjectTo<DoctorDto>(_mapper.ConfigurationProvider)
            .FirstAsync(cancellationToken);

        return Result<DoctorDto>.Success(dto);
    }
}
