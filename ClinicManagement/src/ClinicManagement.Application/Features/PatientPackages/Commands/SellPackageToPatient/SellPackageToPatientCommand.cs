using AutoMapper.QueryableExtensions;
using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Features.PatientPackages.DTOs;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Application.Features.PatientPackages.Commands.SellPackageToPatient;

/// <summary>
/// Raises one upfront Invoice for the package's full price (one InvoiceItem,
/// "10-Session Physiotherapy Package") and creates the PatientPackage that
/// tracks session usage against it. Paying that invoice is a separate step —
/// staff can record cash/card payment the usual way, or send the patient a
/// gateway checkout link (see PaymentsController) — this command only
/// handles "the patient has committed to buying the package", not payment.
/// </summary>
public record SellPackageToPatientCommand(Guid PatientId, Guid TreatmentPackageId, DateTime? DueDate)
    : IRequest<Result<PatientPackageDto>>;

public class SellPackageToPatientCommandValidator : AbstractValidator<SellPackageToPatientCommand>
{
    public SellPackageToPatientCommandValidator()
    {
        RuleFor(x => x.PatientId).NotEmpty();
        RuleFor(x => x.TreatmentPackageId).NotEmpty();
    }
}

public class SellPackageToPatientCommandHandler
    : IRequestHandler<SellPackageToPatientCommand, Result<PatientPackageDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly AutoMapper.IMapper _mapper;

    public SellPackageToPatientCommandHandler(
        IUnitOfWork unitOfWork, ICurrentUserService currentUserService, AutoMapper.IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _mapper = mapper;
    }

    public async Task<Result<PatientPackageDto>> Handle(
        SellPackageToPatientCommand request, CancellationToken cancellationToken)
    {
        var catalogPackage = await _unitOfWork.Repository<TreatmentPackage>().Query()
            .FirstOrDefaultAsync(t => t.Id == request.TreatmentPackageId && t.IsActive, cancellationToken);
        if (catalogPackage is null)
        {
            return Result<PatientPackageDto>.Failure("This package is not available for sale.");
        }

        var patientExists = await _unitOfWork.Repository<Patient>().Query()
            .AnyAsync(p => p.Id == request.PatientId, cancellationToken);
        if (!patientExists)
        {
            return Result<PatientPackageDto>.Failure($"Patient with id '{request.PatientId}' was not found.");
        }

        var invoice = new Invoice
        {
            TenantId = _currentUserService.TenantId,
            InvoiceNumber = await GenerateNextInvoiceNumberAsync(cancellationToken),
            PatientId = request.PatientId,
            IssueDate = DateTime.UtcNow,
            DueDate = request.DueDate ?? DateTime.UtcNow,
            SubTotal = catalogPackage.Price,
            Discount = 0,
            Tax = 0,
            Total = catalogPackage.Price,
            Status = InvoiceStatus.Issued
        };
        invoice.Items.Add(new InvoiceItem
        {
            TenantId = _currentUserService.TenantId,
            Description = $"{catalogPackage.Name} ({catalogPackage.SessionsIncluded} sessions)",
            Quantity = 1,
            UnitPrice = catalogPackage.Price,
            Total = catalogPackage.Price
        });
        await _unitOfWork.Repository<Invoice>().AddAsync(invoice, cancellationToken);

        var patientPackage = new PatientPackage
        {
            TenantId = _currentUserService.TenantId,
            PatientId = request.PatientId,
            TreatmentPackageId = catalogPackage.Id,
            SessionsTotal = catalogPackage.SessionsIncluded,
            SessionsUsed = 0,
            PurchasedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(catalogPackage.ValidityDays)
        };
        await _unitOfWork.Repository<PatientPackage>().AddAsync(patientPackage, cancellationToken);

        // Both rows need to exist before we can cross-link them — SaveChanges once first to
        // get real, EF-generated Ids for both, same "parent rows first" pattern used
        // elsewhere (RegisterPatientAccountCommand, etc.) when two new rows reference each other.
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        patientPackage.InvoiceId = invoice.Id;
        _unitOfWork.Repository<PatientPackage>().Update(patientPackage);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var dto = await _unitOfWork.Repository<PatientPackage>().Query()
            .Where(p => p.Id == patientPackage.Id)
            .ProjectTo<PatientPackageDto>(_mapper.ConfigurationProvider)
            .FirstAsync(cancellationToken);

        return Result<PatientPackageDto>.Success(dto);
    }

    /// <summary>Same trade-off as CreateInvoiceCommand's identical helper — see the note there.</summary>
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
