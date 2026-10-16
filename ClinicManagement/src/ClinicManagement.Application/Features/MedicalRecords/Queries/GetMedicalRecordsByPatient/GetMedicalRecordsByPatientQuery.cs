using AutoMapper.QueryableExtensions;
using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Features.MedicalRecords.DTOs;
using ClinicManagement.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Application.Features.MedicalRecords.Queries.GetMedicalRecordsByPatient;

public record GetMedicalRecordsByPatientQuery(Guid PatientId) : IRequest<List<MedicalRecordDto>>;

public class GetMedicalRecordsByPatientQueryHandler
    : IRequestHandler<GetMedicalRecordsByPatientQuery, List<MedicalRecordDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly AutoMapper.IMapper _mapper;

    public GetMedicalRecordsByPatientQueryHandler(IUnitOfWork unitOfWork, AutoMapper.IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<List<MedicalRecordDto>> Handle(GetMedicalRecordsByPatientQuery request, CancellationToken cancellationToken)
    {
        return await _unitOfWork.Repository<MedicalRecord>().Query()
            .Where(r => r.PatientId == request.PatientId)
            .OrderByDescending(r => r.CreatedAt)
            .ProjectTo<MedicalRecordDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }
}
