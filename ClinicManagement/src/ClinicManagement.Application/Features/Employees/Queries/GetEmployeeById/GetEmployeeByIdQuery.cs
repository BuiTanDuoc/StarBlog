using AutoMapper.QueryableExtensions;
using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Features.Employees.DTOs;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Application.Features.Employees.Queries.GetEmployeeById;

public record GetEmployeeByIdQuery(Guid Id) : IRequest<Result<EmployeeDto>>;

public class GetEmployeeByIdQueryHandler : IRequestHandler<GetEmployeeByIdQuery, Result<EmployeeDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly AutoMapper.IMapper _mapper;

    public GetEmployeeByIdQueryHandler(IUnitOfWork unitOfWork, AutoMapper.IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<EmployeeDto>> Handle(GetEmployeeByIdQuery request, CancellationToken cancellationToken)
    {
        var dto = await _unitOfWork.Repository<Employee>().Query()
            .Where(e => e.Id == request.Id)
            .ProjectTo<EmployeeDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken);

        return dto is null
            ? Result<EmployeeDto>.Failure($"Employee with id '{request.Id}' was not found.")
            : Result<EmployeeDto>.Success(dto);
    }
}
