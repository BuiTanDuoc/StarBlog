using CodeLab.Share.ViewModels.Response;
using FreeSql;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StarBlog.Data.Models;
using StarBlog.Web.Extensions;
using StarBlog.Web.Services;
using StarBlog.Web.ViewModels;
using StarBlog.Web.ViewModels.Blog;

namespace StarBlog.Web.Apis.Blog;

/// <summary>
/// Blog
/// </summary>
[ApiController]
[Route("Api/[controller]")]
[ApiExplorerSettings(GroupName = ApiGroups.Blog)]
public class BlogController : ControllerBase {
    private readonly ILogger<BlogController> _logger;
    private readonly BlogService _blogService;


    public BlogController(BlogService blogService, ILogger<BlogController> logger) {
        _blogService = blogService;
        _logger = logger;
    }

    /// <summary>
    /// Lấy blog được ghim
    /// </summary>
    /// <returns></returns>
    [HttpGet("Top")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<Post?>))]
    public async Task<Post?> GetTopOnePost() {
        return await _blogService.GetTopOnePost();
    }

    /// <summary>
    /// Lấy blog nổi bật
    /// </summary>
    /// <returns></returns>
    [HttpGet("Featured")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<List<Post>>))]
    public async Task<List<Post>> GetFeaturedPosts() {
        return await _blogService.GetFeaturedPosts();
    }

    /// <summary>
    /// Tổng quan thông tin blog
    /// </summary>
    /// <returns></returns>
    // [Authorize]
    [HttpGet("[action]")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<BlogOverview>))]
    public async Task<BlogOverview> Overview() {
        return await _blogService.Overview();
    }

    /// <summary>
    /// Danh sách trạng thái bài viết blog
    /// </summary>
    /// <returns></returns>
    [HttpGet("[action]")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<string?>))]
    public async Task<List<string?>> GetStatusList() {
        return await _blogService.GetStatusList();
    }

    /// <summary>
    /// Tải lên gói nén blog + nhập
    /// </summary>
    /// <returns></returns>
    [Authorize]
    [HttpPost("[action]")]
    public async Task<ApiResponse<Post>> Upload([FromForm] PostCreationDto dto, IFormFile file,
        [FromServices] CategoryService categoryService
    ) {
        if (!file.FileName.EndsWith(".zip")) {
            return ApiResponse.BadRequest("Chỉ có thể tải lên tệp định dạng zip thôi nhé~");
        }

        var category = await categoryService.GetById(dto.CategoryId);
        if (category == null) return ApiResponse.BadRequest($"Danh mục {dto.CategoryId} không tồn tại!");

        try {
            return new ApiResponse<Post>(await _blogService.Upload(dto, file));
        }
        catch (Exception ex) {
            _logger.LogError(ex, "Lỗi khi giải nén tệp: {message}", ex.Message);
            return ApiResponse.Error($"Lỗi khi giải nén tệp: {ex.Message}");
        }
    }
}