using AutoMapper.QueryableExtensions;
using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Services;
using ClinicManagement.Application.Features.Appointments.DTOs;
using ClinicManagement.Application.Features.Appointments.Events;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Application.Features.Appointments.Commands.CreateAppointment;

public record CreateAppointmentCommand : IRequest<Result<AppointmentDto>>
{
    public Guid PatientId { get; init; }
    public Guid DoctorId { get; init; }
    public Guid BranchId { get; init; }
    public DateTime ScheduledStart { get; init; }
    public DateTime ScheduledEnd { get; init; }
    public string? Notes { get; init; }

    /// <summary>When set, this visit redeems one session from the patient's prepaid PatientPackage instead of being billed separately — see the handler's redemption block.</summary>
    public Guid? PatientPackageId { get; init; }
}

public class CreateAppointmentCommandValidator : AbstractValidator<CreateAppointmentCommand>
{
    public CreateAppointmentCommandValidator()
    {
        RuleFor(x => x.PatientId).NotEmpty();
        RuleFor(x => x.DoctorId).NotEmpty();
        RuleFor(x => x.BranchId).NotEmpty();
        RuleFor(x => x.ScheduledStart).NotEmpty().GreaterThan(DateTime.UtcNow).WithMessage("Cannot book an appointment in the past.");
        RuleFor(x => x.ScheduledEnd).GreaterThan(x => x.ScheduledStart);
        RuleFor(x => x.Notes).MaximumLength(2000);
    }
}

public class CreateAppointmentCommandHandler : IRequestHandler<CreateAppointmentCommand, Result<AppointmentDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly ISubscriptionLimitService _subscriptionLimitService;
    private readonly IAppointmentAvailabilityService _availabilityService;
    private readonly IPublisher _publisher;
    private readonly AutoMapper.IMapper _mapper;

    public CreateAppointmentCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        ISubscriptionLimitService subscriptionLimitService,
        IAppointmentAvailabilityService availabilityService,
        IPublisher publisher,
        AutoMapper.IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _subscriptionLimitService = subscriptionLimitService;
        _availabilityService = availabilityService;
        _publisher = publisher;
        _mapper = mapper;
    }

    public async Task<Result<AppointmentDto>> Handle(CreateAppointmentCommand request, CancellationToken cancellationToken)
    {
        await _subscriptionLimitService.EnsureCanCreateAppointmentThisMonthAsync(cancellationToken);

        // Throws AppointmentConflictException (mapped to HTTP 409 by the global exception
        // middleware, Phase 6) if the slot is outside working hours or double-booked.
        await _availabilityService.EnsureSlotIsAvailableAsync(
            request.DoctorId, request.ScheduledStart, request.ScheduledEnd, cancellationToken: cancellationToken);

        PatientPackage? patientPackage = null;
        if (request.PatientPackageId is { } packageId)
        {
            patientPackage = await _unitOfWork.Repository<PatientPackage>().Query()
                .FirstOrDefaultAsync(p => p.Id == packageId, cancellationToken);

            if (patientPackage is null || patientPackage.PatientId != request.PatientId)
            {
                return Result<AppointmentDto>.Failure("This package does not belong to the selected patient.");
            }
            if (patientPackage.IsExpired)
            {
                return Result<AppointmentDto>.Failure("This package has expired.");
            }
            if (patientPackage.SessionsRemaining <= 0)
            {
                return Result<AppointmentDto>.Failure("This package has no sessions remaining.");
            }

            // Redeemed at booking time, not at completion — if the appointment is later
            // cancelled, CancelAppointmentCommand credits the session back (see there).
            patientPackage.SessionsUsed += 1;
            _unitOfWork.Repository<PatientPackage>().Update(patientPackage);
        }

        var appointment = new Appointment
        {
            TenantId = _currentUserService.TenantId,
            PatientId = request.PatientId,
            DoctorId = request.DoctorId,
            BranchId = request.BranchId,
            ScheduledStart = request.ScheduledStart,
            ScheduledEnd = request.ScheduledEnd,
            Status = AppointmentStatus.Pending,
            Notes = request.Notes,
            PatientPackageId = patientPackage?.Id
        };

        appointment.StatusHistory.Add(new AppointmentStatusHistory
        {
            TenantId = _currentUserService.TenantId,
            FromStatus = AppointmentStatus.Pending,
            ToStatus = AppointmentStatus.Pending,
            ChangedBy = _currentUserService.UserName,
            Reason = "Appointment created"
        });

        await _unitOfWork.Repository<Appointment>().AddAsync(appointment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Notifies the doctor (if their login is linked) via SignalR + persists a Notification row —
        // see AppointmentCreatedEventHandler. Published after SaveChanges so the appointment
        // definitely exists before anyone reacts to it.
        await _publisher.Publish(
            new AppointmentCreatedEvent(appointment.Id, appointment.DoctorId, appointment.PatientId, appointment.ScheduledStart),
            cancellationToken);

        var dto = await _unitOfWork.Repository<Appointment>().Query()
            .Where(a => a.Id == appointment.Id)
            .ProjectTo<AppointmentDto>(_mapper.ConfigurationProvider)
            .FirstAsync(cancellationToken);

        return Result<AppointmentDto>.Success(dto);
    }
}
