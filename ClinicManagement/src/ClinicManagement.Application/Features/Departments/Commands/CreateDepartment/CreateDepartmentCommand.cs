using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Features.Departments.DTOs;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Application.Features.Departments.Commands.CreateDepartment;

public record CreateDepartmentCommand(string Name, Guid BranchId) : IRequest<Result<DepartmentDto>>;

public class CreateDepartmentCommandValidator : AbstractValidator<CreateDepartmentCommand>
{
    public CreateDepartmentCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.BranchId).NotEmpty();
    }
}

public class CreateDepartmentCommandHandler : IRequestHandler<CreateDepartmentCommand, Result<DepartmentDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly AutoMapper.IMapper _mapper;

    public CreateDepartmentCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService, AutoMapper.IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _mapper = mapper;
    }

    public async Task<Result<DepartmentDto>> Handle(CreateDepartmentCommand request, CancellationToken cancellationToken)
    {
        var branchExists = await _unitOfWork.Repository<Branch>().Query()
            .AnyAsync(b => b.Id == request.BranchId, cancellationToken);
        if (!branchExists)
        {
            return Result<DepartmentDto>.Failure($"Branch with id '{request.BranchId}' was not found.");
        }

        var department = new Department
        {
            TenantId = _currentUserService.TenantId,
            Name = request.Name,
            BranchId = request.BranchId
        };

        await _unitOfWork.Repository<Department>().AddAsync(department, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<DepartmentDto>.Success(_mapper.Map<DepartmentDto>(department));
    }
}
