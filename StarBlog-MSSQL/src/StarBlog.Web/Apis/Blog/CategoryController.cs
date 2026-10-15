using AutoMapper;
using CodeLab.Share.Extensions;
using CodeLab.Share.ViewModels.Response;
using FreeSql;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StarBlog.Data.Models;
using StarBlog.Web.Extensions;
using StarBlog.Web.Services;
using StarBlog.Web.ViewModels.Categories;

namespace StarBlog.Web.Apis.Blog;

/// <summary>
/// Danh mục bài viết
/// </summary>
[Authorize]
[ApiController]
[Route("Api/[controller]")]
[ApiExplorerSettings(GroupName = ApiGroups.Blog)]
public class CategoryController : ControllerBase {
    private readonly CategoryService _cService;
    private readonly IMapper _mapper;
    private readonly IBaseRepository<Post> _postRepo;

    public CategoryController(CategoryService cService, IMapper mapper, IBaseRepository<Post> postRepo) {
        _cService = cService;
        _mapper = mapper;
        _postRepo = postRepo;
    }

    /// <summary>
    /// Lấy cây thư mục danh mục
    /// </summary>
    [AllowAnonymous]
    [HttpGet("Nodes")]
    public async Task<List<CategoryNode>?> GetNodes() {
        return await _cService.GetNodes();
    }

    [AllowAnonymous]
    [HttpGet("All")]
    public async Task<List<Category>> GetAll() {
        return await _cService.GetAll();
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<ApiResponsePaged<Category>> GetList(int page = 1, int pageSize = 10) {
        var paged = await _cService.GetPagedList(page, pageSize);
        return new ApiResponsePaged<Category> {
            Pagination = paged.ToPaginationMetadata(),
            Data = paged.ToList()
        };
    }

    [AllowAnonymous]
    [HttpGet("{id:int}")]
    public async Task<ApiResponse<Category>> Get(int id) {
        var item = await _cService.GetById(id);
        return item == null ? ApiResponse.NotFound() : new ApiResponse<Category> {Data = item};
    }

    [HttpPost]
    public async Task<ApiResponse<Category>> Add(CategoryCreationDto dto) {
        var item = _mapper.Map<Category>(dto);
        return new ApiResponse<Category>(await _cService.AddOrUpdate(item));
    }

    [HttpPut("{id:int}")]
    public async Task<ApiResponse<Category>> Update(int id, [FromBody] CategoryCreationDto dto) {
        var item = await _cService.GetById(id);
        if (item == null) return ApiResponse.NotFound();

        item = _mapper.Map(dto, item);
        return new ApiResponse<Category>(await _cService.AddOrUpdate(item));
    }

    [HttpDelete("{id:int}")]
    public async Task<ApiResponse> Delete(int id) {
        var item = await _cService.GetById(id);
        if (item == null) return ApiResponse.NotFound();

        if (await _postRepo.Where(a => a.CategoryId == id).AnyAsync())
            return ApiResponse.BadRequest("Danh mục đã chọn còn bài viết, không thể xóa!");

        var rows = await _cService.Delete(item);
        return ApiResponse.Ok($"Đã xóa {rows} bản ghi");
    }


    /// <summary>
    /// Đám mây từ khóa danh mục
    /// </summary>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpGet("[action]")]
    public async Task<List<object>> WordCloud() {
        return await _cService.GetWordCloud();
    }

    /// <summary>
    /// Đặt làm danh mục nổi bật
    /// </summary>
    /// <seealso href="https://fontawesome.com/search?m=free">Thư viện biểu tượng FontAwesome</seealso>
    /// <param name="id"></param>
    /// <param name="dto">Thông tin nổi bật <see cref="FeaturedCategoryCreationDto"/></param>
    /// <returns></returns>
    [HttpPost("{id:int}/[action]")]
    public async Task<ApiResponse<FeaturedCategory>> SetFeatured(int id, [FromBody] FeaturedCategoryCreationDto dto) {
        var item = await _cService.GetById(id);
        return item == null
            ? ApiResponse.NotFound($"Danh mục {id} không tồn tại")
            : new ApiResponse<FeaturedCategory>(await _cService.AddOrUpdateFeaturedCategory(item, dto));
    }

    /// <summary>
    /// Hủy danh mục nổi bật
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpPost("{id:int}/[action]")]
    public async Task<ApiResponse> CancelFeatured(int id) {
        var item = await _cService.GetById(id);
        if (item == null) return ApiResponse.NotFound($"Danh mục {id} không tồn tại");
        var rows = await _cService.DeleteFeaturedCategory(item);
        return ApiResponse.Ok($"delete {rows} rows.");
    }

    /// <summary>
    /// Đặt danh mục hiển thị
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpPost("{id:int}/[action]")]
    public async Task<ApiResponse> SetVisible(int id) {
        var item = await _cService.GetById(id);
        if (item == null) return ApiResponse.NotFound($"Danh mục {id} không tồn tại");
        var rows = _cService.SetVisibility(item, true);
        return ApiResponse.Ok($"affect {rows} rows.");
    }

    /// <summary>
    /// Đặt danh mục ẩn
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpPost("{id:int}/[action]")]
    public async Task<ApiResponse> SetInvisible(int id) {
        var item = await _cService.GetById(id);
        if (item == null) return ApiResponse.NotFound($"Danh mục {id} không tồn tại");
        var rows = await _cService.SetVisibility(item, false);
        return ApiResponse.Ok($"affect {rows} rows.");
    }
}