namespace ClinicManagement.Application.Features.Reports.DTOs;

public record PatientsReportRowDto(
    string PatientCode,
    string FullName,
    string Gender,
    DateTime DateOfBirth,
    string Phone,
    DateTime RegisteredOn,
    int TotalAppointments);

public record AppointmentsReportRowDto(
    DateTime ScheduledStart,
    string PatientName,
    string DoctorName,
    string BranchName,
    string Status);

public record RevenueReportRowDto(
    DateTime PaidAt,
    string PatientName,
    string InvoiceNumber,
    decimal Amount,
    string Method);

public record DoctorPerformanceReportRowDto(
    string DoctorName,
    string Specialization,
    int TotalAppointments,
    int CompletedAppointments,
    int CancelledAppointments,
    int NoShowAppointments,
    decimal TotalRevenue);

/// <summary>SuperAdmin-only, spans every tenant on the platform.</summary>
public record SubscriptionReportRowDto(
    string ClinicName,
    string PlanName,
    DateTime StartDate,
    DateTime EndDate,
    string Status,
    decimal TotalPaid);
