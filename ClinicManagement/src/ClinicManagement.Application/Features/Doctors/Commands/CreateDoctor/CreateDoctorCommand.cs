using AutoMapper.QueryableExtensions;
using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Services;
using ClinicManagement.Application.Features.Doctors.DTOs;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Application.Features.Doctors.Commands.CreateDoctor;

public record CreateDoctorCommand : IRequest<Result<DoctorDto>>
{
    public string FullName { get; init; } = default!;
    public string Specialization { get; init; } = default!;
    public string LicenseNumber { get; init; } = default!;
    public decimal ConsultationFee { get; init; }

    /// <summary>JSON array: [{ "Day": "Monday", "Start": "09:00", "End": "17:00" }, ...]</summary>
    public string WorkScheduleJson { get; init; } = "[]";

    public int YearsOfExperience { get; init; }
    public Guid BranchId { get; init; }
    public Guid? DepartmentId { get; init; }
    public Guid? ApplicationUserId { get; init; }
}

public class CreateDoctorCommandValidator : AbstractValidator<CreateDoctorCommand>
{
    public CreateDoctorCommandValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Specialization).NotEmpty().MaximumLength(150);
        RuleFor(x => x.LicenseNumber).NotEmpty().MaximumLength(100);
        RuleFor(x => x.ConsultationFee).GreaterThanOrEqualTo(0);
        RuleFor(x => x.YearsOfExperience).GreaterThanOrEqualTo(0);
        RuleFor(x => x.BranchId).NotEmpty();
    }
}

public class CreateDoctorCommandHandler : IRequestHandler<CreateDoctorCommand, Result<DoctorDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ISubscriptionLimitService _subscriptionLimitService;
    private readonly AutoMapper.IMapper _mapper;

    public CreateDoctorCommandHandler(
        IUnitOfWork unitOfWork, ICurrentUserService currentUserService,
        ISubscriptionLimitService subscriptionLimitService, AutoMapper.IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _subscriptionLimitService = subscriptionLimitService;
        _mapper = mapper;
    }

    public async Task<Result<DoctorDto>> Handle(CreateDoctorCommand request, CancellationToken cancellationToken)
    {
        await _subscriptionLimitService.EnsureCanAddDoctorAsync(cancellationToken);

        var doctor = new Doctor
        {
            TenantId = _currentUserService.TenantId,
            FullName = request.FullName,
            Specialization = request.Specialization,
            LicenseNumber = request.LicenseNumber,
            ConsultationFee = request.ConsultationFee,
            WorkScheduleJson = request.WorkScheduleJson,
            YearsOfExperience = request.YearsOfExperience,
            BranchId = request.BranchId,
            DepartmentId = request.DepartmentId,
            ApplicationUserId = request.ApplicationUserId
        };

        await _unitOfWork.Repository<Doctor>().AddAsync(doctor, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var dto = await _unitOfWork.Repository<Doctor>().Query()
            .Where(d => d.Id == doctor.Id)
            .ProjectTo<DoctorDto>(_mapper.ConfigurationProvider)
            .FirstAsync(cancellationToken);

        return Result<DoctorDto>.Success(dto);
    }
}
