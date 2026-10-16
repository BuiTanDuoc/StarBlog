using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Features.Invoices.Events;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Application.Features.Invoices.Commands.RecordGatewayPayment;

/// <summary>
/// Deliberately not built on ICurrentUserService.TenantId the way
/// RecordPaymentCommand is — a gateway webhook arrives with no JWT, no
/// authenticated user, no ambient tenant. TenantId here comes from the
/// checkout session's own metadata (set when the session was created —
/// see CreateInvoiceCheckoutCommand), verified against the invoice it
/// claims to be paying so a forged/mismatched TenantId can't record a
/// payment against the wrong tenant's invoice.
/// </summary>
public record RecordGatewayPaymentCommand(Guid TenantId, Guid InvoiceId, decimal Amount, string ProviderReference)
    : IRequest<Result>;

public class RecordGatewayPaymentCommandHandler : IRequestHandler<RecordGatewayPaymentCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPublisher _publisher;

    public RecordGatewayPaymentCommandHandler(IUnitOfWork unitOfWork, IPublisher publisher)
    {
        _unitOfWork = unitOfWork;
        _publisher = publisher;
    }

    public async Task<Result> Handle(RecordGatewayPaymentCommand request, CancellationToken cancellationToken)
    {
        var invoiceRepo = _unitOfWork.Repository<Invoice>();
        var invoice = await invoiceRepo.Query()
            .IgnoreQueryFilters()
            .Include(i => i.Payments)
            .FirstOrDefaultAsync(i => i.Id == request.InvoiceId && i.TenantId == request.TenantId, cancellationToken);

        if (invoice is null)
        {
            return Result.Failure($"Invoice '{request.InvoiceId}' was not found for tenant '{request.TenantId}'.");
        }

        // Idempotency guard: Stripe (and most gateways) can deliver the same webhook event
        // more than once. A payment already recorded with this exact provider reference means
        // this is a redelivery, not a second payment — succeed without inserting twice.
        var alreadyRecorded = invoice.Payments.Any(p => p.TransactionReference == request.ProviderReference);
        if (alreadyRecorded)
        {
            return Result.Success();
        }

        if (invoice.Status is InvoiceStatus.Paid or InvoiceStatus.Void)
        {
            // Already settled by some other means (e.g. cash at the counter) between
            // checkout-session creation and this webhook arriving — not an error, just
            // nothing further to do. The money still arrived; a clinic reconciling Stripe's
            // own dashboard against this system is out of scope here.
            return Result.Success();
        }

        var payment = new Payment
        {
            TenantId = request.TenantId,
            InvoiceId = invoice.Id,
            Amount = request.Amount,
            Method = PaymentMethod.Card,
            PaidAt = DateTime.UtcNow,
            TransactionReference = request.ProviderReference,
            ReceivedBy = "Online payment (gateway webhook)"
        };
        await _unitOfWork.Repository<Payment>().AddAsync(payment, cancellationToken);

        var totalPaid = invoice.Payments.Sum(p => p.Amount) + request.Amount;
        invoice.Status = totalPaid >= invoice.Total ? InvoiceStatus.Paid : InvoiceStatus.PartiallyPaid;
        invoiceRepo.Update(invoice);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _publisher.Publish(
            new PaymentReceivedEvent(request.TenantId, invoice.Id, invoice.PatientId, request.Amount), cancellationToken);

        return Result.Success();
    }
}
