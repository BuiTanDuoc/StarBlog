using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Features.TreatmentPackages.DTOs;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Entities;
using FluentValidation;
using MediatR;

namespace ClinicManagement.Application.Features.TreatmentPackages.Commands.UpdateTreatmentPackage;

public record UpdateTreatmentPackageCommand(
    Guid Id, string Name, string? Description, int SessionsIncluded, decimal Price, int ValidityDays, bool IsActive)
    : IRequest<Result<TreatmentPackageDto>>;

public class UpdateTreatmentPackageCommandValidator : AbstractValidator<UpdateTreatmentPackageCommand>
{
    public UpdateTreatmentPackageCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.SessionsIncluded).GreaterThan(0);
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0);
        RuleFor(x => x.ValidityDays).GreaterThan(0);
    }
}

public class UpdateTreatmentPackageCommandHandler
    : IRequestHandler<UpdateTreatmentPackageCommand, Result<TreatmentPackageDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly AutoMapper.IMapper _mapper;

    public UpdateTreatmentPackageCommandHandler(IUnitOfWork unitOfWork, AutoMapper.IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<TreatmentPackageDto>> Handle(
        UpdateTreatmentPackageCommand request, CancellationToken cancellationToken)
    {
        var repository = _unitOfWork.Repository<TreatmentPackage>();
        var package = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (package is null)
        {
            return Result<TreatmentPackageDto>.Failure($"Package with id '{request.Id}' was not found.");
        }

        // Deliberately doesn't touch SessionsIncluded/Price retroactively on packages already
        // sold to patients (PatientPackage snapshots its own SessionsTotal at sale time) — this
        // only changes what future sales look like.
        package.Name = request.Name;
        package.Description = request.Description;
        package.SessionsIncluded = request.SessionsIncluded;
        package.Price = request.Price;
        package.ValidityDays = request.ValidityDays;
        package.IsActive = request.IsActive;

        repository.Update(package);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<TreatmentPackageDto>.Success(_mapper.Map<TreatmentPackageDto>(package));
    }
}
