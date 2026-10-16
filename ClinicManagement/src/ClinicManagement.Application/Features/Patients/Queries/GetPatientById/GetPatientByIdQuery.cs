using AutoMapper.QueryableExtensions;
using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Features.Patients.DTOs;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Application.Features.Patients.Queries.GetPatientById;

public record GetPatientByIdQuery(Guid Id) : IRequest<Result<PatientDto>>;

public class GetPatientByIdQueryHandler : IRequestHandler<GetPatientByIdQuery, Result<PatientDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly AutoMapper.IMapper _mapper;

    public GetPatientByIdQueryHandler(IUnitOfWork unitOfWork, AutoMapper.IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<PatientDto>> Handle(GetPatientByIdQuery request, CancellationToken cancellationToken)
    {
        var dto = await _unitOfWork.Repository<Patient>().Query()
            .Where(p => p.Id == request.Id)
            .ProjectTo<PatientDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken);

        return dto is null
            ? Result<PatientDto>.Failure($"Patient with id '{request.Id}' was not found.")
            : Result<PatientDto>.Success(dto);
    }
}
