using AutoMapper.QueryableExtensions;
using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Features.PatientPackages.DTOs;
using ClinicManagement.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Application.Features.PatientPackages.Queries.GetPatientPackagesList;

public record GetPatientPackagesListQuery(Guid PatientId) : IRequest<List<PatientPackageDto>>;

public class GetPatientPackagesListQueryHandler
    : IRequestHandler<GetPatientPackagesListQuery, List<PatientPackageDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly AutoMapper.IMapper _mapper;

    public GetPatientPackagesListQueryHandler(IUnitOfWork unitOfWork, AutoMapper.IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<List<PatientPackageDto>> Handle(
        GetPatientPackagesListQuery request, CancellationToken cancellationToken)
    {
        return await _unitOfWork.Repository<PatientPackage>().Query()
            .Where(p => p.PatientId == request.PatientId)
            .OrderByDescending(p => p.PurchasedAt)
            .ProjectTo<PatientPackageDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }
}
