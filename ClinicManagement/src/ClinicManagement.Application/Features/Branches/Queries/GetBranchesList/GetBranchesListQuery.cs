using AutoMapper.QueryableExtensions;
using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Features.Branches.DTOs;
using ClinicManagement.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Application.Features.Branches.Queries.GetBranchesList;

public record GetBranchesListQuery(bool ActiveOnly = false) : IRequest<List<BranchDto>>;

public class GetBranchesListQueryHandler : IRequestHandler<GetBranchesListQuery, List<BranchDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly AutoMapper.IMapper _mapper;

    public GetBranchesListQueryHandler(IUnitOfWork unitOfWork, AutoMapper.IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<List<BranchDto>> Handle(GetBranchesListQuery request, CancellationToken cancellationToken)
    {
        var query = _unitOfWork.Repository<Branch>().Query();
        if (request.ActiveOnly) query = query.Where(b => b.IsActive);

        return await query
            .OrderBy(b => b.Name)
            .ProjectTo<BranchDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }
}
