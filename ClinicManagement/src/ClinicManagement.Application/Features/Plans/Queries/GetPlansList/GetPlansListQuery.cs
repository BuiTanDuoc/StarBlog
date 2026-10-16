using AutoMapper.QueryableExtensions;
using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Features.Plans.DTOs;
using ClinicManagement.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Application.Features.Plans.Queries.GetPlansList;

/// <summary>Deliberately has no [Authorize] requirement at the controller — a prospective
/// customer needs to see pricing/limits before they have an account.</summary>
public record GetPlansListQuery(bool ActiveOnly = true) : IRequest<List<PlanDto>>;

public class GetPlansListQueryHandler : IRequestHandler<GetPlansListQuery, List<PlanDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly AutoMapper.IMapper _mapper;

    public GetPlansListQueryHandler(IUnitOfWork unitOfWork, AutoMapper.IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<List<PlanDto>> Handle(GetPlansListQuery request, CancellationToken cancellationToken)
    {
        var query = _unitOfWork.Repository<Plan>().Query();

        if (request.ActiveOnly)
        {
            query = query.Where(p => p.IsActive);
        }

        return await query
            .OrderBy(p => p.Price)
            .ProjectTo<PlanDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }
}
