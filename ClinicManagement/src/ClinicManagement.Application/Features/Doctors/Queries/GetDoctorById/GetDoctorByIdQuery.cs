using AutoMapper.QueryableExtensions;
using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Features.Doctors.DTOs;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Application.Features.Doctors.Queries.GetDoctorById;

public record GetDoctorByIdQuery(Guid Id) : IRequest<Result<DoctorDto>>;

public class GetDoctorByIdQueryHandler : IRequestHandler<GetDoctorByIdQuery, Result<DoctorDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly AutoMapper.IMapper _mapper;

    public GetDoctorByIdQueryHandler(IUnitOfWork unitOfWork, AutoMapper.IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<DoctorDto>> Handle(GetDoctorByIdQuery request, CancellationToken cancellationToken)
    {
        var dto = await _unitOfWork.Repository<Doctor>().Query()
            .Where(d => d.Id == request.Id)
            .ProjectTo<DoctorDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken);

        return dto is null
            ? Result<DoctorDto>.Failure($"Doctor with id '{request.Id}' was not found.")
            : Result<DoctorDto>.Success(dto);
    }
}
