using System.Text.Json;
using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Services;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Application.Features.Appointments.Queries.GetDoctorAvailability;

public record GetDoctorAvailabilityQuery(Guid DoctorId, DateOnly Date, int SlotMinutes = 30)
    : IRequest<List<AvailableSlotDto>>;

public record AvailableSlotDto(DateTime Start, DateTime End);

public class GetDoctorAvailabilityQueryHandler
    : IRequestHandler<GetDoctorAvailabilityQuery, List<AvailableSlotDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetDoctorAvailabilityQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<AvailableSlotDto>> Handle(GetDoctorAvailabilityQuery request, CancellationToken cancellationToken)
    {
        var doctor = await _unitOfWork.Repository<Doctor>().Query()
            .FirstOrDefaultAsync(d => d.Id == request.DoctorId, cancellationToken);

        if (doctor is null) return new List<AvailableSlotDto>();

        var schedule = JsonSerializer.Deserialize<List<WorkScheduleSlot>>(doctor.WorkScheduleJson) ?? new();
        var daySlot = schedule.FirstOrDefault(s =>
            string.Equals(s.Day, request.Date.DayOfWeek.ToString(), StringComparison.OrdinalIgnoreCase));

        if (daySlot is null) return new List<AvailableSlotDto>(); // doctor doesn't work this day

        var dayStart = request.Date.ToDateTime(TimeOnly.Parse(daySlot.Start));
        var dayEnd = request.Date.ToDateTime(TimeOnly.Parse(daySlot.End));

        var dayBoundStart = request.Date.ToDateTime(TimeOnly.MinValue);
        var dayBoundEnd = dayBoundStart.AddDays(1);

        var bookedSlots = await _unitOfWork.Repository<Appointment>().Query()
            .Where(a => a.DoctorId == request.DoctorId
                        && a.Status != AppointmentStatus.Cancelled
                        && a.Status != AppointmentStatus.NoShow
                        && a.ScheduledStart < dayBoundEnd
                        && a.ScheduledEnd > dayBoundStart)
            .Select(a => new { a.ScheduledStart, a.ScheduledEnd })
            .ToListAsync(cancellationToken);

        var slotLength = TimeSpan.FromMinutes(request.SlotMinutes);
        var availableSlots = new List<AvailableSlotDto>();

        for (var slotStart = dayStart; slotStart + slotLength <= dayEnd; slotStart += slotLength)
        {
            var slotEnd = slotStart + slotLength;
            var overlaps = bookedSlots.Any(b => b.ScheduledStart < slotEnd && b.ScheduledEnd > slotStart);

            if (!overlaps)
            {
                availableSlots.Add(new AvailableSlotDto(slotStart, slotEnd));
            }
        }

        return availableSlots;
    }
}
