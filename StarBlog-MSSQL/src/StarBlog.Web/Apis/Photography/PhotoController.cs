using CodeLab.Share.Extensions;
using CodeLab.Share.ViewModels.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StarBlog.Data.Models;
using StarBlog.Web.Extensions;
using StarBlog.Web.Services;
using StarBlog.Web.ViewModels.Photography;

namespace StarBlog.Web.Apis.Photography;

/// <summary>
/// Nhiếp ảnh/Hình ảnh
/// </summary>
[Authorize]
[ApiController]
[Route("Api/[controller]")]
[ApiExplorerSettings(GroupName = ApiGroups.Photo)]
public class PhotoController : ControllerBase {
    private readonly PhotoService _photoService;

    public PhotoController(PhotoService photoService) {
        _photoService = photoService;
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<ApiResponsePaged<Photo>> GetList(int page = 1, int pageSize = 10) {
        var paged = await _photoService.GetPagedList(page, pageSize);
        return new ApiResponsePaged<Photo> {
            Pagination = paged.ToPaginationMetadata(),
            Data = paged.ToList()
        };
    }

    [AllowAnonymous]
    [HttpGet("{id}")]
    public async Task<ApiResponse<Photo>> Get(string id) {
        var photo = await _photoService.GetById(id);
        return photo == null
            ? ApiResponse.NotFound($"Hình ảnh {id} không tồn tại")
            : new ApiResponse<Photo> { Data = photo };
    }

    /// <summary>
    /// Lấy ảnh thu nhỏ theo chiều rộng chỉ định
    /// </summary>
    [AllowAnonymous]
    [HttpGet("{id}/Thumb")]
    public async Task<IActionResult> GetThumb(string id, [FromQuery] int width = 300, [FromQuery] int quality = 85) {
        var data = await _photoService.GetThumb(id, width, quality);
        return new FileContentResult(data, "image/jpeg");
    }

    [HttpPost]
    public async Task<ApiResponse<Photo>> Add([FromForm] PhotoCreationDto dto, IFormFile file) {
        var photo = await _photoService.Add(dto, file);

        return !ModelState.IsValid
            ? ApiResponse.BadRequest(ModelState)
            : new ApiResponse<Photo>(photo);
    }

    [HttpPut("{id}")]
    public async Task<ApiResponse<Photo>> Update(string id, [FromBody] PhotoUpdateDto dto) {
        var photo = await _photoService.GetById(id);
        if (photo == null) return ApiResponse.NotFound($"Hình ảnh {id} không tồn tại");
        dto.Id = id;
        return new ApiResponse<Photo>(await _photoService.Update(dto)) { Message = "Sửa thông tin ảnh thành công." };
    }

    [HttpDelete("{id}")]
    public async Task<ApiResponse> Delete(string id) {
        var photo = await _photoService.GetById(id);
        if (photo == null) return ApiResponse.NotFound($"Hình ảnh {id} không tồn tại");
        var rows = await _photoService.DeleteById(id);
        return rows > 0
            ? ApiResponse.Ok($"deleted {rows} rows.")
            : ApiResponse.Error("deleting failed.");
    }

    /// <summary>
    /// Đặt làm ảnh nổi bật
    /// </summary>
    [HttpPost("{id}/[action]")]
    public async Task<ApiResponse<FeaturedPhoto>> SetFeatured(string id) {
        var photo = await _photoService.GetById(id);
        return photo == null
            ? ApiResponse.NotFound($"Hình ảnh {id} không tồn tại")
            : new ApiResponse<FeaturedPhoto>(await _photoService.AddFeaturedPhoto(photo));
    }

    /// <summary>
    /// Hủy nổi bật
    /// </summary>
    [HttpPost("{id}/[action]")]
    public async Task<ApiResponse> CancelFeatured(string id) {
        var photo = await _photoService.GetById(id);
        if (photo == null) return ApiResponse.NotFound($"Hình ảnh {id} không tồn tại");
        var rows = await _photoService.DeleteFeaturedPhoto(photo);
        return ApiResponse.Ok($"deleted {rows} rows.");
    }

    /// <summary>
    /// Dựng lại dữ liệu thư viện ảnh (quét lại kích thước, v.v. của từng ảnh)
    /// </summary>
    [HttpPost("[action]")]
    public async Task<ApiResponse> ReBuildData() {
        return ApiResponse.Ok(new {
            Rows = await _photoService.ReBuildData()
        }, "Dựng lại dữ liệu thư viện ảnh");
    }

    /// <summary>
    /// Nhập hàng loạt hình ảnh
    /// </summary>
    [HttpPost("[action]")]
    public async Task<ApiResponse<List<Photo>>> BatchImport() {
        var result = await _photoService.BatchImport();
        return new ApiResponse<List<Photo>> {
            Data = result,
            Message = $"Nhập thành công {result.Count} hình ảnh"
        };
    }
}