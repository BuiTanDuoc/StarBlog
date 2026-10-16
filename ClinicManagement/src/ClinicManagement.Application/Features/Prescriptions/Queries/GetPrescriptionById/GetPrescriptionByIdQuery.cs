using AutoMapper.QueryableExtensions;
using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Features.Prescriptions.DTOs;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Application.Features.Prescriptions.Queries.GetPrescriptionById;

public record GetPrescriptionByIdQuery(Guid Id) : IRequest<Result<PrescriptionDto>>;

public class GetPrescriptionByIdQueryHandler : IRequestHandler<GetPrescriptionByIdQuery, Result<PrescriptionDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly AutoMapper.IMapper _mapper;

    public GetPrescriptionByIdQueryHandler(IUnitOfWork unitOfWork, AutoMapper.IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<PrescriptionDto>> Handle(GetPrescriptionByIdQuery request, CancellationToken cancellationToken)
    {
        var dto = await _unitOfWork.Repository<Prescription>().Query()
            .Where(p => p.Id == request.Id)
            .ProjectTo<PrescriptionDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken);

        return dto is null
            ? Result<PrescriptionDto>.Failure($"Prescription with id '{request.Id}' was not found.")
            : Result<PrescriptionDto>.Success(dto);
    }
}
