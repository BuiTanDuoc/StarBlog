using ClinicManagement.API.Common;
using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Features.FileAttachments.Commands.DeleteFile;
using ClinicManagement.Application.Features.FileAttachments.Commands.UploadFile;
using ClinicManagement.Application.Features.FileAttachments.Queries.GetFileForDownload;
using ClinicManagement.Application.Features.FileAttachments.Queries.GetFilesByEntity;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagement.API.Controllers;

public class FilesController : ApiControllerBase
{
    private readonly IFileStorageService _fileStorageService;

    public FilesController(IFileStorageService fileStorageService)
    {
        _fileStorageService = fileStorageService;
    }

    [HttpGet("by-entity")]
    public async Task<IActionResult> GetByEntity([FromQuery] string entityName, [FromQuery] Guid entityId) =>
        Ok(await Mediator.Send(new GetFilesByEntityQuery(entityName, entityId)));

    [HttpPost]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(25 * 1024 * 1024)]
    public async Task<IActionResult> Upload(
        [FromForm] UploadFileRequest request)
    {
        if (request.File.Length == 0)
        {
            return BadRequest(new
            {
                succeeded = false,
                errors = new[] { "File is empty." }
            });
        }

        await using var stream = request.File.OpenReadStream();

        var command = new UploadFileCommand
        {
            FileName = request.File.FileName,
            ContentType = request.File.ContentType,
            Content = stream,
            EntityName = request.EntityName,
            EntityId = request.EntityId,
            MedicalRecordId = request.MedicalRecordId
        };

        return HandleResult(await Mediator.Send(command));
    }

    [HttpGet("{id:guid}/download")]
    public async Task<IActionResult> Download(Guid id)
    {
        var result = await Mediator.Send(new GetFileForDownloadQuery(id));
        if (!result.Succeeded || result.Data is null)
        {
            return NotFound(new { succeeded = false, errors = result.Errors });
        }

        var stream = await _fileStorageService.ReadAsync(result.Data.StoragePath);
        return File(stream, result.Data.ContentType, result.Data.FileName);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id) =>
        HandleResult(await Mediator.Send(new DeleteFileCommand(id)));
}
public sealed class UploadFileRequest
{
    public IFormFile File { get; set; } = default!;
    public string EntityName { get; set; } = string.Empty;
    public Guid EntityId { get; set; }
    public Guid? MedicalRecordId { get; set; }
}