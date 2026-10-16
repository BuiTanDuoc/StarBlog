using ClinicManagement.Application.Features.Appointments.DTOs;

namespace ClinicManagement.Application.Features.Dashboard.DTOs;

public record SubscriptionSummaryDto(string PlanName, DateTime EndDate, string Status, int DaysRemaining);

public record ClinicOwnerDashboardDto(
    int TotalPatients,
    int TotalDoctors,
    int AppointmentsToday,
    decimal RevenueThisMonth,
    SubscriptionSummaryDto? ActiveSubscription);

public record DoctorDashboardDto(
    List<AppointmentDto> TodaysAppointments,
    List<AppointmentDto> UpcomingAppointments);

public record ReceptionistDashboardDto(
    int BookingsToday,
    int CheckInsToday,
    int WaitingPatients);
