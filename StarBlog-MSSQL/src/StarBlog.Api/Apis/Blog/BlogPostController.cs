using AutoMapper;
using CodeLab.Share.Extensions;
using CodeLab.Share.ViewModels.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StarBlog.Content.Utils;
using StarBlog.Data.Models;
using StarBlog.Api.Extensions;
using StarBlog.Application.Services;
using StarBlog.Application.ViewModels;
using StarBlog.Application.ViewModels.Blog;
using StarBlog.Application.Criteria;
using X.PagedList;

namespace StarBlog.Api.Apis.Blog;

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

    public BlogPostController(PostService postService, BlogService blogService, IMapper mapper) {
        _postService = postService;
        _blogService = blogService;
        _mapper = mapper;
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<ApiResponsePaged<PostDto>> GetList([FromQuery] PostQueryParameters param) {
        // Nếu đã đăng nhập thì đặt chế độ quản trị viên
        // todo Sau này sẽ đổi thành xác định quản trị viên dựa trên vai trò
        var adminMode = User.Identity?.IsAuthenticated ?? false;
        var pagedList = await _postService.GetPagedList(param, adminMode);
        return new ApiResponsePaged<PostDto> {
            Message = "Get posts list",
            Data = pagedList.Select(PostDto.From).ToList(),
            Pagination = pagedList.ToPaginationMetadata()
        };
    }

    [AllowAnonymous]
    [HttpGet("{id}")]
    public async Task<ApiResponse<PostDto>> Get(string id) {
        var post = await _postService.GetById(id);
        return post == null ? ApiResponse.NotFound() : new ApiResponse<PostDto>(PostDto.From(post));
    }

    [AllowAnonymous]
    [HttpGet("slug/{slug}")]
    public async Task<ApiResponse<PostDto>> GetBySlug(string slug) {
        var post = await _postService.GetBySlug(slug);
        return post == null ? ApiResponse.NotFound() : new ApiResponse<PostDto>(PostDto.From(post));
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
    public async Task<ApiResponse<PostDto>> Add(PostCreationDto dto,
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

        var saved = await _postService.InsertOrUpdateAsync(post);
        return new ApiResponse<PostDto>(PostDto.From(saved));
    }

    [HttpPut("{id}")]
    public async Task<ApiResponse<PostDto>> Update(string id, PostUpdateDto dto) {
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
        var saved = await _postService.InsertOrUpdateAsync(post);
        return new ApiResponse<PostDto>(PostDto.From(saved));
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
        await using var stream = file.OpenReadStream();
        var imgUrl = await _postService.UploadImage(post, stream, file.FileName);
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
    public async Task<ApiResponse<FeaturedPostDto>> SetFeatured(string id) {
        var post = await _postService.GetById(id);
        return post == null
            ? ApiResponse.NotFound()
            : new ApiResponse<FeaturedPostDto>(FeaturedPostDto.From(await _blogService.AddFeaturedPost(post)));
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
    public async Task<ApiResponse<TopPostDto>> SetTop(string id) {
        var post = await _postService.GetById(id);
        if (post == null) return ApiResponse.NotFound($"Blog {id} không tồn tại");
        var (data, rows) = await _blogService.SetTopPost(post);
        return new ApiResponse<TopPostDto> { Data = TopPostDto.From(data), Message = $"ok. deleted {rows} old topPosts." };
    }
}
