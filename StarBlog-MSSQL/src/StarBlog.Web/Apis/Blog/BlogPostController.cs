using AutoMapper;
using CodeLab.Share.Extensions;
using CodeLab.Share.ViewModels.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StarBlog.Content.Utils;
using StarBlog.Data.Models;
using StarBlog.Web.Extensions;
using StarBlog.Web.Services;
using StarBlog.Web.ViewModels;
using StarBlog.Web.ViewModels.Blog;
using StarBlog.Web.Criteria;
using X.PagedList;

namespace StarBlog.Web.Apis.Blog;

/// <summary>
/// Bài viết
/// </summary>
[Authorize]
[ApiController]
[Route("Api/[controller]")]
[ApiExplorerSettings(GroupName = ApiGroups.Blog)]
public class BlogPostController : ControllerBase {
    private readonly IMapper _mapper;
    private readonly PostService _postService;
    private readonly BlogService _blogService;
    private readonly TranslationService _translationService;

    public BlogPostController(PostService postService, BlogService blogService,
        TranslationService translationService, IMapper mapper) {
        _postService = postService;
        _blogService = blogService;
        _translationService = translationService;
        _mapper = mapper;
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<ApiResponsePaged<Post>> GetList([FromQuery] PostQueryParameters param) {
        // Nếu đã đăng nhập thì đặt chế độ quản trị viên
        // todo Sau này sẽ đổi thành xác định quản trị viên dựa trên vai trò
        var adminMode = User.Identity?.IsAuthenticated ?? false;
        var pagedList = await _postService.GetPagedList(param, adminMode);
        return new ApiResponsePaged<Post> {
            Message = "Get posts list",
            Data = pagedList.ToList(),
            Pagination = pagedList.ToPaginationMetadata()
        };
    }

    [AllowAnonymous]
    [HttpGet("{id}")]
    public async Task<ApiResponse<Post>> Get(string id) {
        var post = await _postService.GetById(id);
        return post == null ? ApiResponse.NotFound() : new ApiResponse<Post>(post);
    }

    [AllowAnonymous]
    [HttpGet("slug/{slug}")]
    public async Task<ApiResponse<Post>> GetBySlug(string slug) {
        var post = await _postService.GetBySlug(slug);
        return post == null ? ApiResponse.NotFound() : new ApiResponse<Post>(post);
    }

    [HttpDelete("{id}")]
    public async Task<ApiResponse> Delete(string id) {
        var post = await _postService.GetById(id);
        if (post == null) return ApiResponse.NotFound($"Blog {id} không tồn tại");
        var rows = await _postService.Delete(id);
        return ApiResponse.Ok($"Đã xóa {rows} blog");
    }

    // todo Khi xuất bản bài viết cần đồng thời đặt trạng thái đã xuất bản
    [HttpPost]
    public async Task<ApiResponse<Post>> Add(PostCreationDto dto,
        [FromServices] CategoryService categoryService) {
        var post = _mapper.Map<Post>(dto);
        var category = await categoryService.GetById(dto.CategoryId);
        if (category == null) return ApiResponse.BadRequest($"Danh mục {dto.CategoryId} không tồn tại!");

        if (!string.IsNullOrWhiteSpace(dto.Slug) && !await _postService.CheckSlugAvailable(dto.Slug)) {
            return ApiResponse.BadRequest("Slug được chỉ định đã được bài viết khác sử dụng!");
        }

        post.Id = GuidUtils.GuidTo16String();
        post.CreationTime = DateTime.Now;
        post.LastUpdateTime = DateTime.Now;
        post.IsPublish = true;

        // Lấy cấu trúc phân cấp của danh mục
        post.Categories = categoryService.GetCategoryBreadcrumb(category);

        return new ApiResponse<Post>(await _postService.InsertOrUpdateAsync(post));
    }

    [HttpPut("{id}")]
    public async Task<ApiResponse<Post>> Update(string id, PostUpdateDto dto) {
        var post = await _postService.GetById(id);
        if (post == null) return ApiResponse.NotFound($"Blog {id} không tồn tại");

        if (!string.IsNullOrWhiteSpace(dto.Slug)) {
            if (dto.Slug!= post.Slug && !await _postService.CheckSlugAvailable(dto.Slug)) {
                return ApiResponse.BadRequest("Slug được chỉ định đã được bài viết khác sử dụng!");
            }
        }

        // mapper.Map(source) Nhận được một đối tượng hoàn toàn mới
        // mapper.Map(source, dest) Sửa đổi trên cơ sở đối tượng dest
        post = _mapper.Map(dto, post);
        post.LastUpdateTime = DateTime.Now;
        return new ApiResponse<Post>(await _postService.InsertOrUpdateAsync(post));
    }

    /// <summary>
    /// Tải ảnh lên
    /// </summary>
    /// <param name="id"></param>
    /// <param name="file"></param>
    /// <returns></returns>
    [HttpPost("{id}/[action]")]
    public async Task<ApiResponse> UploadImage(string id, IFormFile file) {
        var post = await _postService.GetById(id);
        if (post == null) return ApiResponse.NotFound($"Blog {id} không tồn tại");
        var imgUrl = await _postService.UploadImage(post, file);
        return ApiResponse.Ok(new {
            imgUrl,
            imgName = Path.GetFileNameWithoutExtension(imgUrl)
        });
    }

    /// <summary>
    /// Lấy hình ảnh trong bài viết
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}/[action]")]
    public async Task<ApiResponse<List<string>>> Images(string id) {
        var post = await _postService.GetById(id);
        if (post == null) return ApiResponse.NotFound($"Blog {id} không tồn tại");
        return new ApiResponse<List<string>>(_postService.GetImages(post));
    }

    /// <summary>
    /// Đặt làm blog nổi bật
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpPost("{id}/[action]")]
    public async Task<ApiResponse<FeaturedPost>> SetFeatured(string id) {
        var post = await _postService.GetById(id);
        return post == null
            ? ApiResponse.NotFound()
            : new ApiResponse<FeaturedPost>(await _blogService.AddFeaturedPost(post));
    }

    /// <summary>
    /// Hủy blog nổi bật
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpPost("{id}/[action]")]
    public async Task<ApiResponse> CancelFeatured(string id) {
        var post = await _postService.GetById(id);
        if (post == null) return ApiResponse.NotFound($"Blog {id} không tồn tại");
        var rows = await _blogService.DeleteFeaturedPost(post);
        return ApiResponse.Ok($"delete {rows} rows.");
    }

    /// <summary>
    /// Đặt ghim (chỉ được có một bài ghim)
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpPost("{id}/[action]")]
    public async Task<ApiResponse<TopPost>> SetTop(string id) {
        var post = await _postService.GetById(id);
        if (post == null) return ApiResponse.NotFound($"Blog {id} không tồn tại");
        var (data, rows) = await _blogService.SetTopPost(post);
        return new ApiResponse<TopPost> { Data = data, Message = $"ok. deleted {rows} old topPosts." };
    }

    #region Dịch thuật

    /// <summary>
    /// Dịch bài viết
    /// </summary>
    [HttpPost("{id}/[action]")]
    public async Task<ApiResponse> Translate(string id, [FromQuery] string language = "en") {
        var post = await _postService.GetById(id);
        if (post == null) return ApiResponse.NotFound($"Blog {id} không tồn tại");

        try {
            var translation = await _translationService.TranslatePostAsync(id, language);
            return ApiResponse.Ok(new {
                translation.Id,
                translation.Language,
                translation.Title
            }, $"Dịch bài viết hoàn tất: {translation.Title}");
        }
        catch (Exception ex) {
            return ApiResponse.BadRequest($"Dịch thất bại: {ex.Message}");
        }
    }

    /// <summary>
    /// Lấy bản dịch của bài viết
    /// </summary>
    [AllowAnonymous]
    [HttpGet("{id}/[action]")]
    public async Task<ApiResponse<PostTranslation>> GetTranslation(string id, [FromQuery] string language = "en") {
        var translation = await _translationService.GetTranslation(id, language);
        return translation == null
            ? ApiResponse.NotFound($"Không tìm thấy bản dịch {language}")
            : new ApiResponse<PostTranslation>(translation);
    }

    /// <summary>
    /// Lấy danh sách ngôn ngữ dịch khả dụng của bài viết
    /// </summary>
    [AllowAnonymous]
    [HttpGet("{id}/[action]")]
    public async Task<ApiResponse<List<string>>> AvailableTranslations(string id) {
        var languages = await _translationService.GetAvailableLanguages(id);
        return new ApiResponse<List<string>>(languages);
    }

    /// <summary>
    /// Xóa bản dịch
    /// </summary>
    [HttpDelete("{id}/[action]/{translationId}")]
    public async Task<ApiResponse> DeleteTranslation(string id, string translationId) {
        var rows = await _translationService.DeleteTranslation(translationId);
        return rows > 0
            ? ApiResponse.Ok($"Đã xóa bản dịch")
            : ApiResponse.NotFound("Bản dịch không tồn tại");
    }

    #endregion
}
