using AutoMapper.QueryableExtensions;
using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Features.Appointments.DTOs;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Application.Features.Appointments.Queries.GetAppointmentById;

public record GetAppointmentByIdQuery(Guid Id) : IRequest<Result<AppointmentDto>>;

public class GetAppointmentByIdQueryHandler : IRequestHandler<GetAppointmentByIdQuery, Result<AppointmentDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly AutoMapper.IMapper _mapper;

    public GetAppointmentByIdQueryHandler(IUnitOfWork unitOfWork, AutoMapper.IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<AppointmentDto>> Handle(GetAppointmentByIdQuery request, CancellationToken cancellationToken)
    {
        var dto = await _unitOfWork.Repository<Appointment>().Query()
            .Where(a => a.Id == request.Id)
            .ProjectTo<AppointmentDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken);

        return dto is null
            ? Result<AppointmentDto>.Failure($"Appointment with id '{request.Id}' was not found.")
            : Result<AppointmentDto>.Success(dto);
    }
}
