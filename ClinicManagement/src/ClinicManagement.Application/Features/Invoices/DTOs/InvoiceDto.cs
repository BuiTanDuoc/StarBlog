using AutoMapper;
using ClinicManagement.Application.Common.Mappings;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;

namespace ClinicManagement.Application.Features.Invoices.DTOs;

public class InvoiceItemDto : IMapFrom<InvoiceItem>
{
    public string Description { get; set; } = default!;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Total { get; set; }
}

public class PaymentDto : IMapFrom<Payment>
{
    public Guid Id { get; set; }
    public decimal Amount { get; set; }
    public PaymentMethod Method { get; set; }
    public DateTime PaidAt { get; set; }
    public string? TransactionReference { get; set; }
}

public class InvoiceDto : IMapFrom<Invoice>
{
    public Guid Id { get; set; }
    public string InvoiceNumber { get; set; } = default!;
    public Guid PatientId { get; set; }
    public string PatientName { get; set; } = default!;
    public DateTime IssueDate { get; set; }
    public DateTime DueDate { get; set; }
    public decimal SubTotal { get; set; }
    public decimal Discount { get; set; }
    public decimal Tax { get; set; }
    public decimal Total { get; set; }
    public InvoiceStatus Status { get; set; }
    public decimal AmountPaid { get; set; }
    public decimal Balance { get; set; }
    public List<InvoiceItemDto> Items { get; set; } = new();
    public List<PaymentDto> Payments { get; set; } = new();

    public void Mapping(Profile profile) =>
        profile.CreateMap<Invoice, InvoiceDto>()
            .ForMember(d => d.PatientName, opt => opt.MapFrom(s => s.Patient.FullName))
            .ForMember(d => d.AmountPaid, opt => opt.MapFrom(s => s.Payments.Sum(p => p.Amount)))
            .ForMember(d => d.Balance, opt => opt.MapFrom(s => s.Total - s.Payments.Sum(p => p.Amount)));
}

public class InvoiceListItemDto : IMapFrom<Invoice>
{
    public Guid Id { get; set; }
    public string InvoiceNumber { get; set; } = default!;
    public string PatientName { get; set; } = default!;
    public DateTime IssueDate { get; set; }
    public decimal Total { get; set; }
    public InvoiceStatus Status { get; set; }

    public void Mapping(Profile profile) =>
        profile.CreateMap<Invoice, InvoiceListItemDto>()
            .ForMember(d => d.PatientName, opt => opt.MapFrom(s => s.Patient.FullName));
}
