using System.Text.Json;
using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;
using ClinicManagement.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Application.Common.Services;

public interface IAppointmentAvailabilityService
{
    /// <summary>
    /// Throws AppointmentConflictException if the slot falls outside the
    /// doctor's working hours, or overlaps an existing non-cancelled
    /// appointment for that doctor. excludeAppointmentId is used by
    /// Reschedule so a slot doesn't conflict with itself.
    /// </summary>
    Task EnsureSlotIsAvailableAsync(
        Guid doctorId, DateTime start, DateTime end, Guid? excludeAppointmentId = null, CancellationToken cancellationToken = default);
}

/// <summary>Deserializes Doctor.WorkScheduleJson — see Doctor entity for the expected shape.</summary>
public record WorkScheduleSlot(string Day, string Start, string End);

public class AppointmentAvailabilityService : IAppointmentAvailabilityService
{
    private readonly IUnitOfWork _unitOfWork;

    public AppointmentAvailabilityService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task EnsureSlotIsAvailableAsync(
        Guid doctorId, DateTime start, DateTime end, Guid? excludeAppointmentId = null,
        CancellationToken cancellationToken = default)
    {
        if (end <= start)
        {
            throw new AppointmentConflictException("Appointment end time must be after the start time.");
        }

        var doctor = await _unitOfWork.Repository<Doctor>().Query()
            .FirstOrDefaultAsync(d => d.Id == doctorId, cancellationToken)
            ?? throw new AppointmentConflictException("Doctor not found.");

        EnsureWithinWorkingHours(doctor.WorkScheduleJson, start, end);

        var hasConflict = await _unitOfWork.Repository<Appointment>().Query()
            .Where(a => a.DoctorId == doctorId
                        && a.Id != (excludeAppointmentId ?? Guid.Empty)
                        && a.Status != AppointmentStatus.Cancelled
                        && a.Status != AppointmentStatus.NoShow
                        && a.ScheduledStart < end
                        && a.ScheduledEnd > start)
            .AnyAsync(cancellationToken);

        if (hasConflict)
        {
            throw new AppointmentConflictException(
                $"Dr. {doctor.FullName} already has an appointment overlapping {start:t}–{end:t} on {start:d}.");
        }
    }

    private static void EnsureWithinWorkingHours(string workScheduleJson, DateTime start, DateTime end)
    {
        List<WorkScheduleSlot>? schedule;
        try
        {
            schedule = JsonSerializer.Deserialize<List<WorkScheduleSlot>>(workScheduleJson);
        }
        catch (JsonException)
        {
            return; // malformed/empty schedule — fail open rather than blocking bookings on bad data
        }

        if (schedule is null || schedule.Count == 0) return;

        var daySlot = schedule.FirstOrDefault(s =>
            string.Equals(s.Day, start.DayOfWeek.ToString(), StringComparison.OrdinalIgnoreCase));

        if (daySlot is null)
        {
            throw new AppointmentConflictException($"Doctor does not work on {start.DayOfWeek}.");
        }

        var workStart = start.Date + TimeSpan.Parse(daySlot.Start);
        var workEnd = start.Date + TimeSpan.Parse(daySlot.End);

        if (start < workStart || end > workEnd)
        {
            throw new AppointmentConflictException(
                $"Requested time is outside working hours ({daySlot.Start}–{daySlot.End} on {start.DayOfWeek}).");
        }
    }
}
