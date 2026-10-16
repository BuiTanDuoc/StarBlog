using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Application.Features.FileAttachments.Queries.GetFileForDownload;

public record FileDownloadInfo(string FileName, string ContentType, string StoragePath);

public record GetFileForDownloadQuery(Guid Id) : IRequest<Result<FileDownloadInfo>>;

public class GetFileForDownloadQueryHandler : IRequestHandler<GetFileForDownloadQuery, Result<FileDownloadInfo>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetFileForDownloadQueryHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<Result<FileDownloadInfo>> Handle(GetFileForDownloadQuery request, CancellationToken cancellationToken)
    {
        var info = await _unitOfWork.Repository<FileAttachment>().Query()
            .Where(f => f.Id == request.Id)
            .Select(f => new FileDownloadInfo(f.FileName, f.ContentType, f.StoragePath))
            .FirstOrDefaultAsync(cancellationToken);

        return info is null
            ? Result<FileDownloadInfo>.Failure($"File with id '{request.Id}' was not found.")
            : Result<FileDownloadInfo>.Success(info);
    }
}
