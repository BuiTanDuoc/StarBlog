using AutoMapper.QueryableExtensions;
using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Mappings;
using ClinicManagement.Application.Features.Invoices.DTOs;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;
using ClinicManagement.Shared.Common;
using MediatR;

namespace ClinicManagement.Application.Features.Invoices.Queries.GetInvoicesList;

public record GetInvoicesListQuery : IRequest<PaginatedList<InvoiceListItemDto>>
{
    public Guid? PatientId { get; init; }
    public InvoiceStatus? Status { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

public class GetInvoicesListQueryHandler : IRequestHandler<GetInvoicesListQuery, PaginatedList<InvoiceListItemDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly AutoMapper.IMapper _mapper;

    public GetInvoicesListQueryHandler(IUnitOfWork unitOfWork, AutoMapper.IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PaginatedList<InvoiceListItemDto>> Handle(GetInvoicesListQuery request, CancellationToken cancellationToken)
    {
        var query = _unitOfWork.Repository<Invoice>().Query();

        if (request.PatientId.HasValue) query = query.Where(i => i.PatientId == request.PatientId);
        if (request.Status.HasValue) query = query.Where(i => i.Status == request.Status);

        return await query
            .OrderByDescending(i => i.IssueDate)
            .ProjectTo<InvoiceListItemDto>(_mapper.ConfigurationProvider)
            .ToPaginatedListAsync(request.PageNumber, request.PageSize);
    }
}
