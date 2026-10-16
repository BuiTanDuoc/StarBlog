using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Features.FileAttachments.DTOs;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Entities;
using FluentValidation;
using MediatR;

namespace ClinicManagement.Application.Features.FileAttachments.Commands.UploadFile;

/// <summary>Built by the controller from an IFormFile — not bound directly from a JSON body.</summary>
public record UploadFileCommand : IRequest<Result<FileAttachmentDto>>
{
    public string FileName { get; init; } = default!;
    public string ContentType { get; init; } = default!;
    public Stream Content { get; init; } = default!;
    public string EntityName { get; init; } = default!;
    public Guid EntityId { get; init; }
    public Guid? MedicalRecordId { get; init; }
}

public class UploadFileCommandValidator : AbstractValidator<UploadFileCommand>
{
    private static readonly string[] AllowedContentTypes =
    {
        "application/pdf", "image/jpeg", "image/png", "image/webp",
        "application/msword", "application/vnd.openxmlformats-officedocument.wordprocessingml.document"
    };

    public UploadFileCommandValidator()
    {
        RuleFor(x => x.FileName).NotEmpty().MaximumLength(300);
        RuleFor(x => x.EntityName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.EntityId).NotEmpty();
        RuleFor(x => x.ContentType).Must(ct => AllowedContentTypes.Contains(ct))
            .WithMessage("Unsupported file type. Allowed: PDF, JPEG, PNG, WEBP, DOC, DOCX.");
    }
}

public class UploadFileCommandHandler : IRequestHandler<UploadFileCommand, Result<FileAttachmentDto>>
{
    private const long MaxSizeBytes = 25 * 1024 * 1024; // 25 MB — matches the "Secure File Upload" spec item; adjust per plan storage limit if enforcing per-tenant quotas later.

    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IFileStorageService _fileStorageService;
    private readonly AutoMapper.IMapper _mapper;

    public UploadFileCommandHandler(
        IUnitOfWork unitOfWork, ICurrentUserService currentUserService, IFileStorageService fileStorageService, AutoMapper.IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _fileStorageService = fileStorageService;
        _mapper = mapper;
    }

    public async Task<Result<FileAttachmentDto>> Handle(UploadFileCommand request, CancellationToken cancellationToken)
    {
        if (request.Content.Length > MaxSizeBytes)
        {
            return Result<FileAttachmentDto>.Failure($"File exceeds the maximum allowed size of {MaxSizeBytes / 1024 / 1024} MB.");
        }

        var stored = await _fileStorageService.SaveAsync(
            _currentUserService.TenantId, request.FileName, request.Content, cancellationToken);

        var attachment = new FileAttachment
        {
            TenantId = _currentUserService.TenantId,
            FileName = request.FileName,
            StoragePath = stored.StoragePath,
            ContentType = request.ContentType,
            SizeBytes = stored.SizeBytes,
            EntityName = request.EntityName,
            EntityId = request.EntityId,
            MedicalRecordId = request.MedicalRecordId,
            UploadedBy = _currentUserService.UserName
        };

        await _unitOfWork.Repository<FileAttachment>().AddAsync(attachment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<FileAttachmentDto>.Success(_mapper.Map<FileAttachmentDto>(attachment));
    }
}
