using AutoMapper.QueryableExtensions;
using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Mappings;
using ClinicManagement.Application.Features.Patients.DTOs;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Shared.Common;
using MediatR;

namespace ClinicManagement.Application.Features.Patients.Queries.GetPatientsList;

public record GetPatientsListQuery : IRequest<PaginatedList<PatientListItemDto>>
{
    /// <summary>Matches against FullName, PatientCode, Phone, or NationalId.</summary>
    public string? SearchTerm { get; init; }

    public string? SortBy { get; init; } = "FullName";
    public bool SortDescending { get; init; }

    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

public class GetPatientsListQueryHandler
    : IRequestHandler<GetPatientsListQuery, PaginatedList<PatientListItemDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly AutoMapper.IMapper _mapper;

    public GetPatientsListQueryHandler(IUnitOfWork unitOfWork, AutoMapper.IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PaginatedList<PatientListItemDto>> Handle(
        GetPatientsListQuery request, CancellationToken cancellationToken)
    {
        var query = _unitOfWork.Repository<Patient>().Query();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.Trim();
            query = query.Where(p =>
                p.FullName.Contains(term) ||
                p.PatientCode.Contains(term) ||
                p.Phone.Contains(term) ||
                (p.NationalId != null && p.NationalId.Contains(term)));
        }

        query = ApplySort(query, request.SortBy, request.SortDescending);

        return await query
            .ProjectTo<PatientListItemDto>(_mapper.ConfigurationProvider)
            .ToPaginatedListAsync(request.PageNumber, request.PageSize);
    }

    private static IQueryable<Patient> ApplySort(IQueryable<Patient> query, string? sortBy, bool descending) =>
        sortBy?.ToLowerInvariant() switch
        {
            "createdat" => descending ? query.OrderByDescending(p => p.CreatedAt) : query.OrderBy(p => p.CreatedAt),
            "dateofbirth" => descending ? query.OrderByDescending(p => p.DateOfBirth) : query.OrderBy(p => p.DateOfBirth),
            "patientcode" => descending ? query.OrderByDescending(p => p.PatientCode) : query.OrderBy(p => p.PatientCode),
            _ => descending ? query.OrderByDescending(p => p.FullName) : query.OrderBy(p => p.FullName)
        };
}
