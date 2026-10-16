namespace ClinicManagement.Domain.Enums;

public enum PlanTier
{
    Free = 0,
    Starter = 1,
    Professional = 2,
    Enterprise = 3
}

public enum SubscriptionStatus
{
    Trialing = 0,
    Active = 1,
    PastDue = 2,
    Expired = 3,
    Cancelled = 4,

    /// <summary>
    /// A paid-plan checkout session was created but the gateway hasn't confirmed
    /// payment yet (see CreateSubscriptionCheckoutCommand / ActivateSubscriptionCommand).
    /// Doesn't count as a valid subscription for SubscriptionGuardMiddleware — the
    /// tenant stays on whatever subscription it had before until the webhook fires.
    /// </summary>
    PendingPayment = 5
}

public enum AppointmentStatus
{
    Pending = 0,
    Confirmed = 1,
    CheckedIn = 2,
    InProgress = 3,
    Completed = 4,
    Cancelled = 5,
    NoShow = 6
}

public enum DoctorStatus
{
    Active = 0,
    OnLeave = 1,
    Suspended = 2,
    Inactive = 3
}

public enum EmployeeStatus
{
    Active = 0,
    OnLeave = 1,
    Terminated = 2
}

public enum Gender
{
    Male = 0,
    Female = 1,
    Other = 2
}

public enum BloodGroup
{
    Unknown = 0,
    APositive = 1,
    ANegative = 2,
    BPositive = 3,
    BNegative = 4,
    ABPositive = 5,
    ABNegative = 6,
    OPositive = 7,
    ONegative = 8
}

public enum PaymentMethod
{
    Cash = 0,
    Card = 1,
    Wallet = 2,
    BankTransfer = 3
}

public enum InvoiceStatus
{
    Draft = 0,
    Issued = 1,
    PartiallyPaid = 2,
    Paid = 3,
    Overdue = 4,
    Void = 5
}

public enum NotificationType
{
    NewAppointment = 0,
    AppointmentCancelled = 1,
    AppointmentRescheduled = 2,
    PaymentReceived = 3,
    SubscriptionExpiring = 4,
    SubscriptionExpired = 5,
    General = 6,
    AppointmentReminder = 7
}

public enum AuditAction
{
    Create = 0,
    Update = 1,
    Delete = 2,
    Login = 3,
    Logout = 4
}
