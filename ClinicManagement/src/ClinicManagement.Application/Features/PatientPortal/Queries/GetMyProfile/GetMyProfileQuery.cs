using AutoMapper;
using AutoMapper.QueryableExtensions;
using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Features.Patients.DTOs;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Application.Features.PatientPortal.Queries.GetMyProfile;

public record GetMyProfileQuery : IRequest<Result<PatientDto>>;

public class GetMyProfileQueryHandler : IRequestHandler<GetMyProfileQuery, Result<PatientDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;

    public GetMyProfileQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _mapper = mapper;
    }

    public async Task<Result<PatientDto>> Handle(GetMyProfileQuery request, CancellationToken cancellationToken)
    {
        if (_currentUserService.PatientId is not { } patientId)
        {
            return Result<PatientDto>.Failure("This account is not linked to a patient record.");
        }

        var dto = await _unitOfWork.Repository<Patient>().Query()
            .Where(p => p.Id == patientId)
            .ProjectTo<PatientDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken);

        return dto is null
            ? Result<PatientDto>.Failure("Patient record not found.")
            : Result<PatientDto>.Success(dto);
    }
}
