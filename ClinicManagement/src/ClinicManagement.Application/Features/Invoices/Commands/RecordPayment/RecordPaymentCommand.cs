using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Features.Invoices.Events;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Application.Features.Invoices.Commands.RecordPayment;

public record RecordPaymentCommand(Guid InvoiceId, decimal Amount, PaymentMethod Method, string? TransactionReference)
    : IRequest<Result<Guid>>;

public class RecordPaymentCommandValidator : AbstractValidator<RecordPaymentCommand>
{
    public RecordPaymentCommandValidator()
    {
        RuleFor(x => x.InvoiceId).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0);
    }
}

public class RecordPaymentCommandHandler : IRequestHandler<RecordPaymentCommand, Result<Guid>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IPublisher _publisher;

    public RecordPaymentCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService, IPublisher publisher)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _publisher = publisher;
    }

    public async Task<Result<Guid>> Handle(RecordPaymentCommand request, CancellationToken cancellationToken)
    {
        var invoiceRepo = _unitOfWork.Repository<Invoice>();
        var invoice = await invoiceRepo.Query()
            .Include(i => i.Payments)
            .FirstOrDefaultAsync(i => i.Id == request.InvoiceId, cancellationToken);

        if (invoice is null)
        {
            return Result<Guid>.Failure($"Invoice with id '{request.InvoiceId}' was not found.");
        }

        if (invoice.Status is InvoiceStatus.Paid or InvoiceStatus.Void)
        {
            return Result<Guid>.Failure($"Cannot record a payment against an invoice that is already '{invoice.Status}'.");
        }

        var payment = new Payment
        {
            TenantId = _currentUserService.TenantId,
            InvoiceId = invoice.Id,
            Amount = request.Amount,
            Method = request.Method,
            PaidAt = DateTime.UtcNow,
            TransactionReference = request.TransactionReference,
            ReceivedBy = _currentUserService.UserName
        };
        await _unitOfWork.Repository<Payment>().AddAsync(payment, cancellationToken);

        var totalPaid = invoice.Payments.Sum(p => p.Amount) + request.Amount;
        invoice.Status = totalPaid >= invoice.Total ? InvoiceStatus.Paid : InvoiceStatus.PartiallyPaid;
        invoiceRepo.Update(invoice);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _publisher.Publish(
            new PaymentReceivedEvent(invoice.TenantId, invoice.Id, invoice.PatientId, request.Amount), cancellationToken);

        return Result<Guid>.Success(payment.Id);
    }
}
