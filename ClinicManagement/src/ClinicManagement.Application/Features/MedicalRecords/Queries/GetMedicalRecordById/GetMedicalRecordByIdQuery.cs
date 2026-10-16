using AutoMapper.QueryableExtensions;
using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Features.MedicalRecords.DTOs;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Application.Features.MedicalRecords.Queries.GetMedicalRecordById;

public record GetMedicalRecordByIdQuery(Guid Id) : IRequest<Result<MedicalRecordDto>>;

public class GetMedicalRecordByIdQueryHandler : IRequestHandler<GetMedicalRecordByIdQuery, Result<MedicalRecordDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly AutoMapper.IMapper _mapper;

    public GetMedicalRecordByIdQueryHandler(IUnitOfWork unitOfWork, AutoMapper.IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<MedicalRecordDto>> Handle(GetMedicalRecordByIdQuery request, CancellationToken cancellationToken)
    {
        var dto = await _unitOfWork.Repository<MedicalRecord>().Query()
            .Where(r => r.Id == request.Id)
            .ProjectTo<MedicalRecordDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken);

        return dto is null
            ? Result<MedicalRecordDto>.Failure($"Medical record with id '{request.Id}' was not found.")
            : Result<MedicalRecordDto>.Success(dto);
    }
}
