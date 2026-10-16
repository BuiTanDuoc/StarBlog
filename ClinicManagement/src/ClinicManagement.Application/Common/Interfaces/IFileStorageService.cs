namespace ClinicManagement.Application.Common.Interfaces;

public record StoredFileResult(string StoragePath, long SizeBytes);

/// <summary>
/// Abstracts where file bytes actually live. The default Infrastructure
/// implementation writes to local disk under a per-tenant folder — fine for
/// a single-instance deployment, but swap for an S3/Azure Blob
/// implementation before running more than one API instance, since local
/// disk storage won't be shared across instances/containers.
/// </summary>
public interface IFileStorageService
{
    Task<StoredFileResult> SaveAsync(Guid tenantId, string fileName, Stream content, CancellationToken cancellationToken = default);

    Task<Stream> ReadAsync(string storagePath, CancellationToken cancellationToken = default);

    Task DeleteAsync(string storagePath, CancellationToken cancellationToken = default);
}
