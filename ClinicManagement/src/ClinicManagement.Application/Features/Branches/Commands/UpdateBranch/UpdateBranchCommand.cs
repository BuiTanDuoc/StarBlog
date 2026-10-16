using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Features.Branches.DTOs;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Entities;
using FluentValidation;
using MediatR;

namespace ClinicManagement.Application.Features.Branches.Commands.UpdateBranch;

public record UpdateBranchCommand : IRequest<Result<BranchDto>>
{
    public Guid Id { get; init; }
    public string Name { get; init; } = default!;
    public string Address { get; init; } = default!;
    public string Phone { get; init; } = default!;
    public string? Email { get; init; }
    public bool IsActive { get; init; }
}

public class UpdateBranchCommandValidator : AbstractValidator<UpdateBranchCommand>
{
    public UpdateBranchCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Address).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Phone).NotEmpty().MaximumLength(30);
        RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrEmpty(x.Email));
    }
}

public class UpdateBranchCommandHandler : IRequestHandler<UpdateBranchCommand, Result<BranchDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly AutoMapper.IMapper _mapper;

    public UpdateBranchCommandHandler(IUnitOfWork unitOfWork, AutoMapper.IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<BranchDto>> Handle(UpdateBranchCommand request, CancellationToken cancellationToken)
    {
        var repository = _unitOfWork.Repository<Branch>();
        var branch = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (branch is null)
        {
            return Result<BranchDto>.Failure($"Branch with id '{request.Id}' was not found.");
        }

        branch.Name = request.Name;
        branch.Address = request.Address;
        branch.Phone = request.Phone;
        branch.Email = request.Email;
        branch.IsActive = request.IsActive;

        repository.Update(branch);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<BranchDto>.Success(_mapper.Map<BranchDto>(branch));
    }
}
