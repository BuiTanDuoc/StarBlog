using ClinicManagement.Application.Common.Mappings;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;

namespace ClinicManagement.Application.Features.Expenses.DTOs;

public class ExpenseDto : IMapFrom<Expense>
{
    public Guid Id { get; set; }
    public Guid? BranchId { get; set; }
    public string Category { get; set; } = default!;
    public string? Description { get; set; }
    public decimal Amount { get; set; }
    public DateTime ExpenseDate { get; set; }
    public PaymentMethod Method { get; set; }
    public string? ApprovedBy { get; set; }
}
