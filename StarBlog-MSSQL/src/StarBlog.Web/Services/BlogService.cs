using System.IO.Compression;
using System.Text;
using FreeSql;
using StarBlog.Content.Utils;
using StarBlog.Data.Models;
using StarBlog.Content;
using StarBlog.Web.ViewModels.Blog;

namespace StarBlog.Web.Services;

public class BlogService {
    private readonly IWebHostEnvironment _environment;
    private readonly IBaseRepository<Post> _postRepo;
    private readonly IBaseRepository<Category> _categoryRepo;
    private readonly IBaseRepository<Photo> _photoRepo;
    private readonly IBaseRepository<TopPost> _topPostRepo;
    private readonly IBaseRepository<FeaturedPost> _fPostRepo;
    private readonly IBaseRepository<FeaturedCategory> _fCategoryRepo;
    private readonly IBaseRepository<FeaturedPhoto> _fPhotoRepo;

    public BlogService(IBaseRepository<TopPost> topPostRepo, IBaseRepository<FeaturedPost> fPostRepo, IBaseRepository<Post> postRepo,
        IBaseRepository<Category> categoryRepo, IBaseRepository<Photo> photoRepo, IBaseRepository<FeaturedCategory> fCategoryRepo,
        IBaseRepository<FeaturedPhoto> fPhotoRepo, IWebHostEnvironment environment) {
        _topPostRepo = topPostRepo;
        _fPostRepo = fPostRepo;
        _postRepo = postRepo;
        _categoryRepo = categoryRepo;
        _photoRepo = photoRepo;
        _fCategoryRepo = fCategoryRepo;
        _fPhotoRepo = fPhotoRepo;
        _environment = environment;
    }

    /// <summary>
    /// Lấy tổng quan thông tin blog
    /// </summary>
    /// <returns></returns>
    public async Task<BlogOverview> Overview() {
        return new BlogOverview {
            PostsCount = await _postRepo.Select.CountAsync(),
            CategoriesCount = await _categoryRepo.Select.CountAsync(),
            PhotosCount = await _photoRepo.Select.CountAsync(),
            FeaturedPostsCount = await _fPostRepo.Select.CountAsync(),
            FeaturedCategoriesCount = await _fCategoryRepo.Select.CountAsync(),
            FeaturedPhotosCount = await _fPhotoRepo.Select.CountAsync()
        };
    }

    public async Task<Post?> GetTopOnePost() {
        return (await _topPostRepo.Select.Include(a => a.Post.Category).FirstAsync())?.Post;
    }

    /// <summary>
    /// Lấy các hàng (row) blog nổi bật, mỗi hàng tối đa hai blog
    /// </summary>
    /// <returns></returns>
    [Obsolete("Không cần tách thành row nữa, dùng trực tiếp GetFeaturedPosts() là được")]
    public async Task<List<List<Post>>> GetFeaturedPostRows() {
        var data = new List<List<Post>>();

        var posts = await GetFeaturedPosts();
        for (var i = 0; i < posts.Count; i += 2) {
            data.Add(new List<Post> {posts[i], posts[i + 1]});
        }

        return data;
    }

    public async Task<List<Post>> GetFeaturedPosts() {
        return await _fPostRepo.Select.Include(a => a.Post.Category)
            .ToListAsync(a => a.Post);
    }

    public async Task<FeaturedPost> AddFeaturedPost(Post post) {
        var item = await _fPostRepo.Where(a => a.PostId == post.Id).FirstAsync();
        if (item != null) return item;
        item = new FeaturedPost {PostId = post.Id};
        await _fPostRepo.InsertAsync(item);
        return item;
    }

    public async Task<int> DeleteFeaturedPost(Post post) {
        var items = await _fPostRepo.Where(a => a.PostId == post.Id).CountAsync();
        return items == 0 ? 0 : await _fPostRepo.Where(a => a.PostId == post.Id).ToDelete().ExecuteAffrowsAsync();
    }

    /// <summary>
    /// Đặt blog được ghim
    /// </summary>
    /// <param name="post"></param>
    /// <returns>Trả về đối tượng <see cref="TopPost"/> và số dòng blog ghim cũ đã bị xóa</returns>
    public async Task<(TopPost, int)> SetTopPost(Post post) {
        var rows = await _topPostRepo.Select.ToDelete().ExecuteAffrowsAsync();
        var item = new TopPost {PostId = post.Id};
        await _topPostRepo.InsertAsync(item);
        return (item, rows);
    }

    /// <summary>
    /// Lấy danh sách trạng thái của bài viết
    /// </summary>
    /// <returns></returns>
    public async Task<List<string?>> GetStatusList() {
        return await _postRepo.Select.GroupBy(a => a.Status)
            .ToListAsync(a => a.Key);
    }

    /// <summary>
    /// Tải lên blog
    /// todo Đã hoàn thành sơ bộ chức năng này, nhưng còn quá nhiều mã thừa, cần tối ưu
    /// </summary>
    /// <returns></returns>
    public async Task<Post> Upload(PostCreationDto dto, IFormFile file) {
        var tempFile = Path.GetTempFileName();
        await using (var fs = new FileStream(tempFile, FileMode.Create)) {
            await file.CopyToAsync(fs);
        }

        var extractPath = Path.Combine(Path.GetTempPath(), "StarBlog", Guid.NewGuid().ToString());

        // Giải nén bằng bảng mã chỉ định để tránh lỗi font tên tệp tiếng Trung
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        ZipFile.ExtractToDirectory(tempFile, extractPath, Encoding.GetEncoding(dto.ZipEncoding));

        var dir = new DirectoryInfo(extractPath);
        var files = dir.GetFiles("*.md");
        var mdFile = files.First();

        using var reader = mdFile.OpenText();
        var content = await reader.ReadToEndAsync();
        var post = new Post {
            Id = GuidUtils.GuidTo16String(),
            Status = "Đã xuất bản",
            Title = dto.Title ?? $"{DateTime.Now.ToLongDateString()} Bài viết",
            Summary = dto.Summary,
            IsPublish = true,
            Content = content,
            Path = "",
            CreationTime = DateTime.Now,
            LastUpdateTime = DateTime.Now,
            CategoryId = dto.CategoryId,
        };

        // Xử lý danh mục nhiều cấp
        var category = await _categoryRepo.Where(a => a.Id == dto.CategoryId).FirstAsync();
        if (category == null) {
            post.Categories = "0";
        }
        else {
            var categories = new List<Category> {category};
            var parent = category.Parent;
            while (parent != null) {
                categories.Add(parent);
                parent = parent.Parent;
            }

            categories.Reverse();
            post.Categories = string.Join(",", categories.Select(a => a.Id));
        }

        var assetsPath = Path.Combine(_environment.WebRootPath, "media", "blog");
        var processor = new PostProcessor(extractPath, assetsPath, post);

        // Xử lý tiêu đề và trạng thái bài viết
        processor.InflateStatusTitle();

        // Xử lý nội dung chính của bài viết
        // Khi nhập bài viết, đồng thời nhập luôn các hình ảnh trong bài viết và thay thế đường dẫn tương đối của hình ảnh
        post.Content = processor.MarkdownParse();
        if (string.IsNullOrEmpty(post.Summary)) {
            post.Summary = processor.GetSummary(200);
        }

        // Lưu vào cơ sở dữ liệu
        post = await _postRepo.InsertAsync(post);

        return post;
    }
}
