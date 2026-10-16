using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Features.Branches.DTOs;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Entities;
using FluentValidation;
using MediatR;

namespace ClinicManagement.Application.Features.Branches.Commands.CreateBranch;

public record CreateBranchCommand : IRequest<Result<BranchDto>>
{
    public string Name { get; init; } = default!;
    public string Address { get; init; } = default!;
    public string Phone { get; init; } = default!;
    public string? Email { get; init; }
}

public class CreateBranchCommandValidator : AbstractValidator<CreateBranchCommand>
{
    public CreateBranchCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Address).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Phone).NotEmpty().MaximumLength(30);
        RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrEmpty(x.Email));
    }
}

public class CreateBranchCommandHandler : IRequestHandler<CreateBranchCommand, Result<BranchDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly AutoMapper.IMapper _mapper;

    public CreateBranchCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService, AutoMapper.IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _mapper = mapper;
    }

    public async Task<Result<BranchDto>> Handle(CreateBranchCommand request, CancellationToken cancellationToken)
    {
        var branch = new Branch
        {
            TenantId = _currentUserService.TenantId,
            Name = request.Name,
            Address = request.Address,
            Phone = request.Phone,
            Email = request.Email,
            IsActive = true
        };

        await _unitOfWork.Repository<Branch>().AddAsync(branch, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<BranchDto>.Success(_mapper.Map<BranchDto>(branch));
    }
}
