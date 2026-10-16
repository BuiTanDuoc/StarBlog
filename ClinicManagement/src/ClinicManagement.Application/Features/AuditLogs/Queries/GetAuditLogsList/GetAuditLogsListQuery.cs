using AutoMapper.QueryableExtensions;
using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Mappings;
using ClinicManagement.Application.Features.AuditLogs.DTOs;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;
using ClinicManagement.Shared.Common;
using MediatR;

namespace ClinicManagement.Application.Features.AuditLogs.Queries.GetAuditLogsList;

public record GetAuditLogsListQuery : IRequest<PaginatedList<AuditLogDto>>
{
    public string? EntityName { get; init; }
    public Guid? UserId { get; init; }
    public AuditAction? Action { get; init; }
    public DateTime? FromDate { get; init; }
    public DateTime? ToDate { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 50;
}

public class GetAuditLogsListQueryHandler : IRequestHandler<GetAuditLogsListQuery, PaginatedList<AuditLogDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly AutoMapper.IMapper _mapper;

    public GetAuditLogsListQueryHandler(IUnitOfWork unitOfWork, AutoMapper.IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PaginatedList<AuditLogDto>> Handle(GetAuditLogsListQuery request, CancellationToken cancellationToken)
    {
        var query = _unitOfWork.Repository<AuditLog>().Query();

        if (!string.IsNullOrWhiteSpace(request.EntityName)) query = query.Where(a => a.EntityName == request.EntityName);
        if (request.UserId.HasValue) query = query.Where(a => a.UserId == request.UserId);
        if (request.Action.HasValue) query = query.Where(a => a.Action == request.Action);
        if (request.FromDate.HasValue) query = query.Where(a => a.Timestamp >= request.FromDate);
        if (request.ToDate.HasValue) query = query.Where(a => a.Timestamp <= request.ToDate);

        return await query
            .OrderByDescending(a => a.Timestamp)
            .ProjectTo<AuditLogDto>(_mapper.ConfigurationProvider)
            .ToPaginatedListAsync(request.PageNumber, request.PageSize);
    }
}
