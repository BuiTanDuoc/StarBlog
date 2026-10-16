using AutoMapper.QueryableExtensions;
using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Features.Invoices.DTOs;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Application.Features.Invoices.Commands.CreateInvoice;

public record CreateInvoiceItemDto(string Description, int Quantity, decimal UnitPrice);

public record CreateInvoiceCommand : IRequest<Result<InvoiceDto>>
{
    public Guid PatientId { get; init; }
    public Guid? AppointmentId { get; init; }
    public DateTime DueDate { get; init; }
    public decimal Discount { get; init; }
    public decimal Tax { get; init; }
    public List<CreateInvoiceItemDto> Items { get; init; } = new();
}

public class CreateInvoiceCommandValidator : AbstractValidator<CreateInvoiceCommand>
{
    public CreateInvoiceCommandValidator()
    {
        RuleFor(x => x.PatientId).NotEmpty();
        RuleFor(x => x.DueDate).NotEmpty();
        RuleFor(x => x.Discount).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Tax).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Items).NotEmpty().WithMessage("An invoice must contain at least one line item.");
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.Description).NotEmpty().MaximumLength(500);
            item.RuleFor(i => i.Quantity).GreaterThan(0);
            item.RuleFor(i => i.UnitPrice).GreaterThanOrEqualTo(0);
        });
    }
}

public class CreateInvoiceCommandHandler : IRequestHandler<CreateInvoiceCommand, Result<InvoiceDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly AutoMapper.IMapper _mapper;

    public CreateInvoiceCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService, AutoMapper.IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _mapper = mapper;
    }

    public async Task<Result<InvoiceDto>> Handle(CreateInvoiceCommand request, CancellationToken cancellationToken)
    {
        var subTotal = request.Items.Sum(i => i.Quantity * i.UnitPrice);
        var total = subTotal - request.Discount + request.Tax;

        var invoice = new Invoice
        {
            TenantId = _currentUserService.TenantId,
            InvoiceNumber = await GenerateNextInvoiceNumberAsync(cancellationToken),
            PatientId = request.PatientId,
            AppointmentId = request.AppointmentId,
            IssueDate = DateTime.UtcNow,
            DueDate = request.DueDate,
            SubTotal = subTotal,
            Discount = request.Discount,
            Tax = request.Tax,
            Total = total,
            Status = InvoiceStatus.Issued
        };

        foreach (var item in request.Items)
        {
            invoice.Items.Add(new InvoiceItem
            {
                TenantId = _currentUserService.TenantId,
                Description = item.Description,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                Total = item.Quantity * item.UnitPrice
            });
        }

        await _unitOfWork.Repository<Invoice>().AddAsync(invoice, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var dto = await _unitOfWork.Repository<Invoice>().Query()
            .Where(i => i.Id == invoice.Id)
            .ProjectTo<InvoiceDto>(_mapper.ConfigurationProvider)
            .FirstAsync(cancellationToken);

        return Result<InvoiceDto>.Success(dto);
    }

    /// <summary>"INV-2026-000001" — see the identical trade-off note on Patient code generation (CreatePatientCommand).</summary>
    private async Task<string> GenerateNextInvoiceNumberAsync(CancellationToken cancellationToken)
    {
        var year = DateTime.UtcNow.Year;
        var count = await _unitOfWork.Repository<Invoice>().Query()
            .IgnoreQueryFilters()
            .Where(i => i.TenantId == _currentUserService.TenantId && i.IssueDate.Year == year)
            .CountAsync(cancellationToken);

        return $"INV-{year}-{(count + 1):D6}";
    }
}
