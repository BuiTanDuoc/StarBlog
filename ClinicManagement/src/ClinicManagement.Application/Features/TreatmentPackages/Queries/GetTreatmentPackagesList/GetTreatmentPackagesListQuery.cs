using AutoMapper.QueryableExtensions;
using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Features.TreatmentPackages.DTOs;
using ClinicManagement.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Application.Features.TreatmentPackages.Queries.GetTreatmentPackagesList;

public record GetTreatmentPackagesListQuery(bool ActiveOnly = true) : IRequest<List<TreatmentPackageDto>>;

public class GetTreatmentPackagesListQueryHandler
    : IRequestHandler<GetTreatmentPackagesListQuery, List<TreatmentPackageDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly AutoMapper.IMapper _mapper;

    public GetTreatmentPackagesListQueryHandler(IUnitOfWork unitOfWork, AutoMapper.IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<List<TreatmentPackageDto>> Handle(
        GetTreatmentPackagesListQuery request, CancellationToken cancellationToken)
    {
        var query = _unitOfWork.Repository<TreatmentPackage>().Query();
        if (request.ActiveOnly) query = query.Where(t => t.IsActive);

        return await query
            .OrderBy(t => t.Name)
            .ProjectTo<TreatmentPackageDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }
}
