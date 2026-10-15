using CodeLab.Share.ViewModels.Response;
using FreeSql;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StarBlog.Data.Models;
using StarBlog.Api.Extensions;
using StarBlog.Application.Services;

namespace StarBlog.Api.Apis.Blog;

/// <summary>
/// Ảnh nổi bật
/// </summary>
[Authorize]
[ApiController]
[Route("Api/[controller]")]
[ApiExplorerSettings(GroupName = ApiGroups.Blog)]
public class FeaturedPhotoController : ControllerBase {
    private readonly PhotoService _photoService;
    private readonly IBaseRepository<FeaturedPhoto> _fpRepo;

    public FeaturedPhotoController(PhotoService photoService, IBaseRepository<FeaturedPhoto> fpRepo) {
        _photoService = photoService;
        _fpRepo = fpRepo;
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<List<FeaturedPhoto>> GetList() {
        return await _fpRepo.Select
            .Include(a => a.Photo)
            .ToListAsync();
    }

    [AllowAnonymous]
    [HttpGet("{id:int}")]
    public async Task<ApiResponse<FeaturedPhoto>> Get(int id) {
        var item = await _fpRepo.Where(a => a.Id == id).FirstAsync();
        return item == null
            ? ApiResponse.NotFound($"Bản ghi ảnh nổi bật {id} không tồn tại")
            : new ApiResponse<FeaturedPhoto>(item);
    }

    [HttpPost]
    public async Task<ApiResponse<FeaturedPhoto>> Add(string photoId) {
        var photo = await _photoService.GetById(photoId);
        return photo == null
            ? ApiResponse.NotFound($"Hình ảnh {photoId} không tồn tại")
            : new ApiResponse<FeaturedPhoto>(await _photoService.AddFeaturedPhoto(photo));
    }

    [HttpDelete("{id:int}")]
    public async Task<ApiResponse> Delete(int id) {
        var item = await _fpRepo.Where(a => a.Id == id).FirstAsync();
        if (item == null) return ApiResponse.NotFound($"Bản ghi ảnh nổi bật {id} không tồn tại");
        var rows = await _fpRepo.DeleteAsync(item);
        return ApiResponse.Ok($"deleted {rows} rows.");
    }
}