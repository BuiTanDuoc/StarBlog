using AutoMapper;
using FreeSql;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;
using StarBlog.Content.Utils;
using StarBlog.Data.Models;
using StarBlog.Application.Abstractions;
using StarBlog.Application.ViewModels.Photography;
using X.PagedList;

namespace StarBlog.Application.Services;

public class PhotoService {
    private readonly IBaseRepository<Photo> _photoRepo;
    private readonly IBaseRepository<FeaturedPhoto> _featuredPhotoRepo;
    private readonly IAppPathProvider _paths;
    private readonly IClock _clock;
    private readonly IMapper _mapper;

    public PhotoService(IBaseRepository<Photo> photoRepo, IAppPathProvider paths, IClock clock,
        IBaseRepository<FeaturedPhoto> featuredPhotoRepo, IMapper mapper) {
        _photoRepo = photoRepo;
        _paths = paths;
        _clock = clock;
        _featuredPhotoRepo = featuredPhotoRepo;
        _mapper = mapper;
    }

    public async Task<List<Photo>> GetAll() {
        return await _photoRepo.Select.ToListAsync();
    }

    public async Task<IPagedList<Photo>> GetPagedList(int page = 1, int pageSize = 10) {
        IPagedList<Photo> pagedList = new StaticPagedList<Photo>(
            await _photoRepo.Select.OrderByDescending(a => a.CreateTime).Page(page, pageSize).ToListAsync(),
            page, pageSize, Convert.ToInt32(await _photoRepo.Select.CountAsync())
        );
        return pagedList;
    }

    public async Task<List<Photo>> GetFeaturedPhotos() {
        return await _featuredPhotoRepo.Select
            .Include(a => a.Photo).ToListAsync(a => a.Photo);
    }

    public async Task<Photo?> GetById(string id) {
        return await _photoRepo.Where(a => a.Id == id).FirstAsync();
    }

    public async Task<Photo?> GetNext(string id) {
        var photo = await _photoRepo.Where(a => a.Id == id).FirstAsync();
        if (photo == null) return null;
        var next = await _photoRepo
            .Where(a => a.CreateTime < photo.CreateTime && a.Id != id)
            .OrderByDescending(a => a.CreateTime)
            .FirstAsync();
        return next;
    }

    public async Task<Photo?> GetPrevious(string id) {
        var photo = await _photoRepo.Where(a => a.Id == id).FirstAsync();
        if (photo == null) return null;
        var next = await _photoRepo
            .Where(a => a.CreateTime > photo.CreateTime && a.Id != id)
            .OrderBy(a => a.CreateTime)
            .FirstAsync();
        return next;
    }

    /// <summary>
    /// Tạo ảnh thu nhỏ JPEG (sử dụng ImageSharp)
    /// </summary>
    /// <param name="id"></param>
    /// <param name="width">Đặt là 0 thì không thay đổi kích thước</param>
    /// <param name="quality">Chất lượng, mặc định 85</param>
    public async Task<byte[]> GetThumb(string id, int width = 0, int quality = 85) {
        var photo = await _photoRepo.Where(a => a.Id == id).FirstAsync();
        using var image = await Image.LoadAsync(GetPhotoFilePath(photo));

        if (width != 0) {
            image.Mutate(x => x.Resize(width, 0));
        }

        using var memoryStream = new MemoryStream();
        var encoder = new JpegEncoder {
            Quality = quality
        };
        await image.SaveAsync(memoryStream, encoder);
        return memoryStream.ToArray();
    }

    public async Task<Photo> Update(PhotoUpdateDto dto) {
        var photo = await GetById(dto.Id);
        photo = _mapper.Map(dto, photo);
        await _photoRepo.UpdateAsync(photo);
        return photo;
    }

    public async Task<Photo> Add(PhotoCreationDto dto, Stream photoStream) {
        var photoId = GuidUtils.GuidTo16String();
        var photo = new Photo {
            Id = photoId,
            Title = dto.Title,
            CreateTime = _clock.Now,
            Location = dto.Location,
            FilePath = $"{photoId}.jpg"
        };

        var savePath = GetPhotoFilePath(photo);

        await using var buffered = new MemoryStream();
        await photoStream.CopyToAsync(buffered);
        buffered.Position = 0;

        var resizeFlag = await ResizePhoto(buffered, savePath);

        if (!resizeFlag) {
            buffered.Position = 0;
            await using var fs = new FileStream(savePath, FileMode.Create, FileAccess.Write, FileShare.None);
            await buffered.CopyToAsync(fs);
        }

        photo = await BuildPhotoData(photo);

        return await _photoRepo.InsertAsync(photo);
    }

    /// <summary>
    /// Lấy một ảnh ngẫu nhiên
    /// </summary>
    public async Task<Photo?> GetRandomPhoto() {
        var count = await _photoRepo.Select.CountAsync();
        if (count == 0) {
            return null;
        }

        return await _photoRepo.Select.Take(1).Offset(Random.Shared.Next((int)count)).FirstAsync();
    }

    /// <summary>
    /// Thêm ảnh nổi bật
    /// </summary>
    public async Task<FeaturedPhoto> AddFeaturedPhoto(Photo photo) {
        var item = await _featuredPhotoRepo.Where(a => a.PhotoId == photo.Id).FirstAsync();
        if (item != null) return item;
        item = new FeaturedPhoto { PhotoId = photo.Id };
        await _featuredPhotoRepo.InsertAsync(item);
        return item;
    }

    /// <summary>
    /// Xóa ảnh nổi bật
    /// </summary>
    public async Task<int> DeleteFeaturedPhoto(Photo photo) {
        var item = await _featuredPhotoRepo.Where(a => a.PhotoId == photo.Id).FirstAsync();
        return item == null ? 0 : await _featuredPhotoRepo.DeleteAsync(item);
    }

    /// <summary>
    /// Xóa ảnh
    /// <para>Xóa tệp ảnh và bản ghi trong cơ sở dữ liệu</para>
    /// </summary>
    public async Task<int> DeleteById(string id) {
        var photo = await _photoRepo.Where(a => a.Id == id).FirstAsync();
        if (photo == null) return -1;

        var filePath = GetPhotoFilePath(photo);
        if (File.Exists(filePath)) File.Delete(filePath);
        return await _photoRepo.DeleteAsync(a => a.Id == id);
    }

    /// <summary>
    /// Dựng lại dữ liệu thư viện ảnh (quét lại kích thước, v.v. của từng ảnh)
    /// </summary>
    public async Task<int> ReBuildData() {
        var photos = await GetAll();
        var photosUpdate = new List<Photo>();
        foreach (var photo in photos) {
            photosUpdate.Add(await BuildPhotoData(photo));
        }

        return await _photoRepo.UpdateAsync(photosUpdate);
    }

    /// <summary>
    /// Nhập hàng loạt hình ảnh
    /// </summary>
    public async Task<List<Photo>> BatchImport() {
        var result = new List<Photo>();
        var importPath = Path.Combine(_paths.WebRootPath, "assets", "photography");
        var root = new DirectoryInfo(importPath);
        foreach (var file in root.GetFiles()) {
            var photoId = GuidUtils.GuidTo16String();
            var filename = Path.GetFileNameWithoutExtension(file.Name);
            var photo = new Photo {
                Id = photoId,
                Title = filename,
                CreateTime = _clock.Now,
                Location = filename,
                FilePath = $"{photoId}.jpg"
            };
            var savePath = GetPhotoFilePath(photo);

            // Nếu ảnh vượt quá giới hạn kích thước thì cần điều chỉnh trước
            var resizeFlag = await ResizePhoto(new FileStream(file.FullName, FileMode.Open), savePath);

            // Nếu chưa điều chỉnh kích thước thì lưu trực tiếp ảnh đã tải lên
            if (!resizeFlag) {
                file.CopyTo(savePath, true);
            }

            photo = await BuildPhotoData(photo);
            await _photoRepo.InsertAsync(photo);
            result.Add(photo);
        }

        return result;
    }

    /// <summary>
    /// Khởi tạo thư mục tài nguyên ảnh
    /// </summary>
    private string InitPhotoMediaDir() {
        var dir = Path.Combine(_paths.WebRootPath, "media", "photography");
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

        return dir;
    }

    /// <summary>
    /// Lấy đường dẫn lưu trữ vật lý của ảnh
    /// </summary>
    private string GetPhotoFilePath(Photo photo) {
        return Path.Combine(InitPhotoMediaDir(), photo.FilePath);
    }

    /// <summary>
    /// Dựng lại dữ liệu ảnh (quét kích thước, v.v. của ảnh)
    /// </summary>
    private async Task<Photo> BuildPhotoData(Photo photo) {
        var savePath = GetPhotoFilePath(photo);
        var imgInfo = await Image.IdentifyAsync(savePath);
        photo.Width = imgInfo.Width;
        photo.Height = imgInfo.Height;

        return photo;
    }

    /// <summary>
    /// Điều chỉnh kích thước ảnh theo thiết lập
    /// </summary>
    private static async Task<bool> ResizePhoto(Stream stream, string savePath) {
        const int maxWidth = 1500;
        const int maxHeight = 1500;
        var resizeFlag = false;

        using var image = await Image.LoadAsync(stream);

        if (image.Width > maxWidth) {
            resizeFlag = true;
            image.Mutate(a => a.Resize(maxWidth, 0));
        }

        if (image.Height > maxHeight) {
            resizeFlag = true;
            image.Mutate(a => a.Resize(0, maxHeight));
        }

        if (resizeFlag) {
            await image.SaveAsync(savePath);
        }

        return resizeFlag;
    }
}
