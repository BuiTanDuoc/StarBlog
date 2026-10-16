using AutoMapper.QueryableExtensions;
using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Features.MedicalRecordTemplates.DTOs;
using ClinicManagement.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Application.Features.MedicalRecordTemplates.Queries.GetMedicalRecordTemplatesList;

public record GetMedicalRecordTemplatesListQuery(string? Specialization = null, bool ActiveOnly = true)
    : IRequest<List<MedicalRecordTemplateDto>>;

public class GetMedicalRecordTemplatesListQueryHandler
    : IRequestHandler<GetMedicalRecordTemplatesListQuery, List<MedicalRecordTemplateDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly AutoMapper.IMapper _mapper;

    public GetMedicalRecordTemplatesListQueryHandler(IUnitOfWork unitOfWork, AutoMapper.IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<List<MedicalRecordTemplateDto>> Handle(
        GetMedicalRecordTemplatesListQuery request, CancellationToken cancellationToken)
    {
        var query = _unitOfWork.Repository<MedicalRecordTemplate>().Query();

        if (request.ActiveOnly) query = query.Where(t => t.IsActive);
        if (!string.IsNullOrWhiteSpace(request.Specialization))
            query = query.Where(t => t.Specialization == request.Specialization);

        return await query
            .OrderBy(t => t.Specialization).ThenBy(t => t.Name)
            .ProjectTo<MedicalRecordTemplateDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }
}
