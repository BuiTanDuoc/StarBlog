using CodeLab.Share.ViewModels.Response;
using FreeSql;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StarBlog.Data.Models;
using StarBlog.Web.Extensions;
using StarBlog.Web.ViewModels;

namespace StarBlog.Web.Apis.Blog;

/// <summary>
/// Blog nổi bật
/// </summary>
[Authorize]
[ApiController]
[Route("Api/[controller]")]
[ApiExplorerSettings(GroupName = ApiGroups.Blog)]
public class FeaturedPostController : ControllerBase {
    private readonly IBaseRepository<Post> _postRepo;
    private readonly IBaseRepository<FeaturedPost> _featuredPostRepo;

    public FeaturedPostController(IBaseRepository<FeaturedPost> featuredPostRepo, IBaseRepository<Post> postRepo) {
        _featuredPostRepo = featuredPostRepo;
        _postRepo = postRepo;
    }

    [AllowAnonymous]
    [HttpGet]
    public ApiResponse<List<FeaturedPost>> GetList() {
        return new ApiResponse<List<FeaturedPost>>(
            _featuredPostRepo.Select.Include(a => a.Post.Category).ToList()
        );
    }

    [AllowAnonymous]
    [HttpGet("{id:int}")]
    public ApiResponse<FeaturedPost> Get(int id) {
        var item = _featuredPostRepo.Where(a => a.Id == id)
            .Include(a => a.Post).First();
        return item == null ? ApiResponse.NotFound() : new ApiResponse<FeaturedPost>(item);
    }

    [HttpPost]
    public ApiResponse<FeaturedPost> Add([FromQuery] string postId) {
        var post = _postRepo.Where(a => a.Id == postId).First();
        if (post == null) return ApiResponse.NotFound($"Blog {postId} không tồn tại");
        var item= _featuredPostRepo.Insert(new FeaturedPost { PostId = postId });
        return new ApiResponse<FeaturedPost>(item);
    }

    [HttpDelete("{id:int}")]
    public ApiResponse Delete(int id) {
        var item = _featuredPostRepo.Where(a => a.Id == id).First();
        if (item == null) return ApiResponse.NotFound($"Bản ghi blog nổi bật {id} không tồn tại");
        var rows = _featuredPostRepo.Delete(item);
        return ApiResponse.Ok($"deleted {rows} rows.");
    }
}