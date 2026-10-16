using AutoMapper.QueryableExtensions;
using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Features.FileAttachments.DTOs;
using ClinicManagement.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Application.Features.FileAttachments.Queries.GetFilesByEntity;

public record GetFilesByEntityQuery(string EntityName, Guid EntityId) : IRequest<List<FileAttachmentDto>>;

public class GetFilesByEntityQueryHandler : IRequestHandler<GetFilesByEntityQuery, List<FileAttachmentDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly AutoMapper.IMapper _mapper;

    public GetFilesByEntityQueryHandler(IUnitOfWork unitOfWork, AutoMapper.IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<List<FileAttachmentDto>> Handle(GetFilesByEntityQuery request, CancellationToken cancellationToken)
    {
        return await _unitOfWork.Repository<FileAttachment>().Query()
            .Where(f => f.EntityName == request.EntityName && f.EntityId == request.EntityId)
            .OrderByDescending(f => f.CreatedAt)
            .ProjectTo<FileAttachmentDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }
}
