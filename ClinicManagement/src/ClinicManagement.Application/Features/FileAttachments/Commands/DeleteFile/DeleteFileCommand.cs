using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Entities;
using FluentValidation;
using MediatR;

namespace ClinicManagement.Application.Features.FileAttachments.Commands.DeleteFile;

public record DeleteFileCommand(Guid Id) : IRequest<Result>;

public class DeleteFileCommandValidator : AbstractValidator<DeleteFileCommand>
{
    public DeleteFileCommandValidator() => RuleFor(x => x.Id).NotEmpty();
}

public class DeleteFileCommandHandler : IRequestHandler<DeleteFileCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileStorageService _fileStorageService;

    public DeleteFileCommandHandler(IUnitOfWork unitOfWork, IFileStorageService fileStorageService)
    {
        _unitOfWork = unitOfWork;
        _fileStorageService = fileStorageService;
    }

    public async Task<Result> Handle(DeleteFileCommand request, CancellationToken cancellationToken)
    {
        var repository = _unitOfWork.Repository<FileAttachment>();
        var attachment = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (attachment is null)
        {
            return Result.Failure($"File with id '{request.Id}' was not found.");
        }

        // Delete the DB row first: if storage deletion fails, a dangling blob is a cheap cleanup
        // job later, whereas a DB row pointing at already-deleted bytes is a 404 waiting to happen.
        repository.Remove(attachment);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _fileStorageService.DeleteAsync(attachment.StoragePath, cancellationToken);

        return Result.Success();
    }
}
