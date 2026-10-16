using AutoMapper.QueryableExtensions;
using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Features.Branches.DTOs;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Application.Features.Branches.Queries.GetBranchById;

public record GetBranchByIdQuery(Guid Id) : IRequest<Result<BranchDto>>;

public class GetBranchByIdQueryHandler : IRequestHandler<GetBranchByIdQuery, Result<BranchDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly AutoMapper.IMapper _mapper;

    public GetBranchByIdQueryHandler(IUnitOfWork unitOfWork, AutoMapper.IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<BranchDto>> Handle(GetBranchByIdQuery request, CancellationToken cancellationToken)
    {
        var dto = await _unitOfWork.Repository<Branch>().Query()
            .Where(b => b.Id == request.Id)
            .ProjectTo<BranchDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken);

        return dto is null
            ? Result<BranchDto>.Failure($"Branch with id '{request.Id}' was not found.")
            : Result<BranchDto>.Success(dto);
    }
}
