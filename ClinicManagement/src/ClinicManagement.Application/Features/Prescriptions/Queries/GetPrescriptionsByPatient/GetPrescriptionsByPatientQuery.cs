using AutoMapper.QueryableExtensions;
using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Features.Prescriptions.DTOs;
using ClinicManagement.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Application.Features.Prescriptions.Queries.GetPrescriptionsByPatient;

public record GetPrescriptionsByPatientQuery(Guid PatientId) : IRequest<List<PrescriptionDto>>;

public class GetPrescriptionsByPatientQueryHandler
    : IRequestHandler<GetPrescriptionsByPatientQuery, List<PrescriptionDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly AutoMapper.IMapper _mapper;

    public GetPrescriptionsByPatientQueryHandler(IUnitOfWork unitOfWork, AutoMapper.IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<List<PrescriptionDto>> Handle(GetPrescriptionsByPatientQuery request, CancellationToken cancellationToken)
    {
        return await _unitOfWork.Repository<Prescription>().Query()
            .Where(p => p.PatientId == request.PatientId)
            .OrderByDescending(p => p.DateIssued)
            .ProjectTo<PrescriptionDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }
}
