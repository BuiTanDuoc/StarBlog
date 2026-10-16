using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Application.Features.Invoices.Commands.CreateInvoiceCheckout;

/// <summary>
/// Shared by staff (InvoicesController) and the patient portal
/// (PatientPortalController) — the caller is responsible for checking the
/// requester is allowed to pay this invoice (staff: Billing.ManagePayments;
/// patient: it's their own invoice) before sending this command.
/// </summary>
public record CreateInvoiceCheckoutCommand(Guid InvoiceId, string SuccessUrl, string CancelUrl)
    : IRequest<Result<CheckoutSessionResult>>;

public class CreateInvoiceCheckoutCommandValidator : AbstractValidator<CreateInvoiceCheckoutCommand>
{
    public CreateInvoiceCheckoutCommandValidator()
    {
        RuleFor(x => x.InvoiceId).NotEmpty();
        RuleFor(x => x.SuccessUrl).NotEmpty();
        RuleFor(x => x.CancelUrl).NotEmpty();
    }
}

public class CreateInvoiceCheckoutCommandHandler
    : IRequestHandler<CreateInvoiceCheckoutCommand, Result<CheckoutSessionResult>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IPaymentGatewayService _paymentGatewayService;

    public CreateInvoiceCheckoutCommandHandler(
        IUnitOfWork unitOfWork, ICurrentUserService currentUserService, IPaymentGatewayService paymentGatewayService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _paymentGatewayService = paymentGatewayService;
    }

    public async Task<Result<CheckoutSessionResult>> Handle(
        CreateInvoiceCheckoutCommand request, CancellationToken cancellationToken)
    {
        var invoice = await _unitOfWork.Repository<Invoice>().Query()
            .Include(i => i.Payments)
            .Include(i => i.Patient)
            .FirstOrDefaultAsync(i => i.Id == request.InvoiceId, cancellationToken);

        if (invoice is null)
        {
            return Result<CheckoutSessionResult>.Failure($"Invoice with id '{request.InvoiceId}' was not found.");
        }

        if (invoice.Status is InvoiceStatus.Paid or InvoiceStatus.Void)
        {
            return Result<CheckoutSessionResult>.Failure($"This invoice is already '{invoice.Status}' — nothing to pay.");
        }

        var balance = invoice.Total - invoice.Payments.Sum(p => p.Amount);
        if (balance <= 0)
        {
            return Result<CheckoutSessionResult>.Failure("This invoice has no outstanding balance.");
        }

        var session = await _paymentGatewayService.CreateCheckoutSessionAsync(new CheckoutSessionRequest(
            TenantId: _currentUserService.TenantId,
            InvoiceId: invoice.Id,
            InvoiceNumber: invoice.InvoiceNumber,
            AmountDue: balance,
            CustomerEmail: invoice.Patient.Email,
            SuccessUrl: request.SuccessUrl,
            CancelUrl: request.CancelUrl),
            cancellationToken);

        return Result<CheckoutSessionResult>.Success(session);
    }
}
