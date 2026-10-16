namespace ClinicManagement.Domain.Exceptions;

/// <summary>
/// Thrown for business-rule violations that should never happen if the
/// Application layer validated correctly (e.g. an invariant enforced inside
/// an entity method). Caught by the global exception middleware and mapped
/// to HTTP 400. Prefer Result&lt;T&gt; failures over throwing where the failure
/// is an expected outcome (e.g. "slot already booked").
/// </summary>
public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
}

public class SubscriptionLimitExceededException : DomainException
{
    public SubscriptionLimitExceededException(string resource, int limit)
        : base($"Subscription plan limit reached for '{resource}' (max: {limit}). Upgrade your plan to continue.")
    {
    }
}

public class TenantMismatchException : DomainException
{
    public TenantMismatchException()
        : base("The requested resource does not belong to the current tenant.")
    {
    }
}

public class AppointmentConflictException : DomainException
{
    public AppointmentConflictException(string message) : base(message) { }
}
