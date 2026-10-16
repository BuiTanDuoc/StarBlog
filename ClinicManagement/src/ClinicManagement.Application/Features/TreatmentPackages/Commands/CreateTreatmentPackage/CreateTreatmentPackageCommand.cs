using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Features.TreatmentPackages.DTOs;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Entities;
using FluentValidation;
using MediatR;

namespace ClinicManagement.Application.Features.TreatmentPackages.Commands.CreateTreatmentPackage;

public record CreateTreatmentPackageCommand(
    string Name, string? Description, int SessionsIncluded, decimal Price, int ValidityDays)
    : IRequest<Result<TreatmentPackageDto>>;

public class CreateTreatmentPackageCommandValidator : AbstractValidator<CreateTreatmentPackageCommand>
{
    public CreateTreatmentPackageCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Description).MaximumLength(1000);
        RuleFor(x => x.SessionsIncluded).GreaterThan(0);
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0);
        RuleFor(x => x.ValidityDays).GreaterThan(0);
    }
}

public class CreateTreatmentPackageCommandHandler
    : IRequestHandler<CreateTreatmentPackageCommand, Result<TreatmentPackageDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly AutoMapper.IMapper _mapper;

    public CreateTreatmentPackageCommandHandler(
        IUnitOfWork unitOfWork, ICurrentUserService currentUserService, AutoMapper.IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _mapper = mapper;
    }

    public async Task<Result<TreatmentPackageDto>> Handle(
        CreateTreatmentPackageCommand request, CancellationToken cancellationToken)
    {
        var package = new TreatmentPackage
        {
            TenantId = _currentUserService.TenantId,
            Name = request.Name,
            Description = request.Description,
            SessionsIncluded = request.SessionsIncluded,
            Price = request.Price,
            ValidityDays = request.ValidityDays,
            IsActive = true
        };

        await _unitOfWork.Repository<TreatmentPackage>().AddAsync(package, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<TreatmentPackageDto>.Success(_mapper.Map<TreatmentPackageDto>(package));
    }
}
