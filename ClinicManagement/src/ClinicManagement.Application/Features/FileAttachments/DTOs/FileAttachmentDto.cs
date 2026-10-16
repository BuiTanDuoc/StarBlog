using ClinicManagement.Application.Common.Mappings;
using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Application.Features.FileAttachments.DTOs;

public class FileAttachmentDto : IMapFrom<FileAttachment>
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = default!;
    public string ContentType { get; set; } = default!;
    public long SizeBytes { get; set; }
    public string EntityName { get; set; } = default!;
    public Guid EntityId { get; set; }
    public string? UploadedBy { get; set; }
    public DateTime CreatedAt { get; set; }
}
