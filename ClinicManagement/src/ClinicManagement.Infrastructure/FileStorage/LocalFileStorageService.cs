using ClinicManagement.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;

namespace ClinicManagement.Infrastructure.FileStorage;

/// <summary>
/// Writes under {RootPath}/{tenantId}/{guid}-{sanitizedFileName}. RootPath
/// comes from configuration ("FileStorage:RootPath") so it can point at a
/// mounted volume in Docker/production rather than the container's
/// ephemeral filesystem.
/// </summary>
public class LocalFileStorageService : IFileStorageService
{
    private readonly string _rootPath;

    public LocalFileStorageService(IConfiguration configuration)
    {
        _rootPath = configuration["FileStorage:RootPath"] ?? Path.Combine(AppContext.BaseDirectory, "uploads");
        Directory.CreateDirectory(_rootPath);
    }

    public async Task<StoredFileResult> SaveAsync(
        Guid tenantId, string fileName, Stream content, CancellationToken cancellationToken = default)
    {
        var tenantFolder = Path.Combine(_rootPath, tenantId.ToString());
        Directory.CreateDirectory(tenantFolder);

        var safeFileName = $"{Guid.NewGuid()}-{SanitizeFileName(fileName)}";
        var fullPath = Path.Combine(tenantFolder, safeFileName);

        await using (var fileStream = new FileStream(fullPath, FileMode.CreateNew, FileAccess.Write))
        {
            await content.CopyToAsync(fileStream, cancellationToken);
        }

        // Stored path is relative to _rootPath (tenantId/fileName), not an absolute filesystem
        // path — keeps the DB portable if RootPath ever moves (new volume, new host).
        var relativePath = Path.Combine(tenantId.ToString(), safeFileName);
        return new StoredFileResult(relativePath, new FileInfo(fullPath).Length);
    }

    public Task<Stream> ReadAsync(string storagePath, CancellationToken cancellationToken = default)
    {
        var fullPath = Path.Combine(_rootPath, storagePath);
        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException("The requested file could not be found.", storagePath);
        }

        Stream stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read);
        return Task.FromResult(stream);
    }

    public Task DeleteAsync(string storagePath, CancellationToken cancellationToken = default)
    {
        var fullPath = Path.Combine(_rootPath, storagePath);
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }
        return Task.CompletedTask;
    }

    private static string SanitizeFileName(string fileName)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        return new string(fileName.Where(c => !invalidChars.Contains(c)).ToArray());
    }
}
