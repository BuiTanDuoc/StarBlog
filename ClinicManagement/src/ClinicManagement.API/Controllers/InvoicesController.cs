using ClinicManagement.API.Common;
using ClinicManagement.Application.Features.Invoices.Commands.CreateInvoice;
using ClinicManagement.Application.Features.Invoices.Commands.CreateInvoiceCheckout;
using ClinicManagement.Application.Features.Invoices.Commands.RecordPayment;
using ClinicManagement.Application.Features.Invoices.Queries.GetInvoiceById;
using ClinicManagement.Application.Features.Invoices.Queries.GetInvoicesList;
using ClinicManagement.Domain.Enums;
using ClinicManagement.Infrastructure.Authorization;
using ClinicManagement.Shared.Constants;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagement.API.Controllers;

public class InvoicesController : ApiControllerBase
{
    [HasPermission(Permissions.Billing.View)]
    [HttpGet]
    public async Task<IActionResult> GetList([FromQuery] GetInvoicesListQuery query) =>
        Ok(await Mediator.Send(query));

    [HasPermission(Permissions.Billing.View)]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id) =>
        HandleResult(await Mediator.Send(new GetInvoiceByIdQuery(id)));

    [HasPermission(Permissions.Billing.ManageInvoices)]
    [HttpPost]
    public async Task<IActionResult> Create(CreateInvoiceCommand command)
    {
        var result = await Mediator.Send(command);
        return HandleCreatedResult(result, nameof(GetById), new { id = result.Data?.Id });
    }

    [HasPermission(Permissions.Billing.ManagePayments)]
    [HttpPost("{id:guid}/payments")]
    public async Task<IActionResult> RecordPayment(Guid id, [FromBody] RecordPaymentBody body) =>
        HandleResult(await Mediator.Send(new RecordPaymentCommand(id, body.Amount, body.Method, body.TransactionReference)));

    /// <summary>Generates a hosted checkout link (e.g. to text/email the patient) rather than collecting a card in-app — this system never touches raw card numbers.</summary>
    [HasPermission(Permissions.Billing.ManagePayments)]
    [HttpPost("{id:guid}/checkout")]
    public async Task<IActionResult> CreateCheckout(Guid id, [FromBody] CreateCheckoutBody body) =>
        HandleResult(await Mediator.Send(new CreateInvoiceCheckoutCommand(id, body.SuccessUrl, body.CancelUrl)));

    public record RecordPaymentBody(decimal Amount, PaymentMethod Method, string? TransactionReference);
    public record CreateCheckoutBody(string SuccessUrl, string CancelUrl);
}
