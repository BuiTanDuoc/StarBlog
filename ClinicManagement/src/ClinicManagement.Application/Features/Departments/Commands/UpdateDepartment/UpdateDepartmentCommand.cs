using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Features.Departments.DTOs;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Entities;
using FluentValidation;
using MediatR;

namespace ClinicManagement.Application.Features.Departments.Commands.UpdateDepartment;

public record UpdateDepartmentCommand(Guid Id, string Name, Guid BranchId) : IRequest<Result<DepartmentDto>>;

public class UpdateDepartmentCommandValidator : AbstractValidator<UpdateDepartmentCommand>
{
    public UpdateDepartmentCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.BranchId).NotEmpty();
    }
}

public class UpdateDepartmentCommandHandler : IRequestHandler<UpdateDepartmentCommand, Result<DepartmentDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly AutoMapper.IMapper _mapper;

    public UpdateDepartmentCommandHandler(IUnitOfWork unitOfWork, AutoMapper.IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<DepartmentDto>> Handle(UpdateDepartmentCommand request, CancellationToken cancellationToken)
    {
        var repository = _unitOfWork.Repository<Department>();
        var department = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (department is null)
        {
            return Result<DepartmentDto>.Failure($"Department with id '{request.Id}' was not found.");
        }

        department.Name = request.Name;
        department.BranchId = request.BranchId;

        repository.Update(department);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<DepartmentDto>.Success(_mapper.Map<DepartmentDto>(department));
    }
}
