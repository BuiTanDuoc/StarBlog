using System.Net;
using FreeSql;
using Markdig;
using Markdig.Renderers.Normalize;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using StarBlog.Content.Utils;
using StarBlog.Data.Models;
using StarBlog.Content.Extensions.Markdown;
using StarBlog.Application.Abstractions;
using StarBlog.Application.Criteria;
using X.PagedList;

namespace StarBlog.Application.Services;

public class PostService {
    private readonly ILogger<PostService> _logger;
    private readonly IBaseRepository<Post> _postRepo;
    private readonly IBaseRepository<Category> _categoryRepo;
    private readonly IAppPathProvider _paths;
    private readonly IFileStorage _fileStorage;
    private readonly ConfigService _conf;
    private readonly CommonService _commonService;
    private readonly IBackgroundTaskQueue _backgroundTaskQueue;
    private readonly IServiceScopeFactory _scopeFactory;

    private string Host => _conf["host"];

    public PostService(IBaseRepository<Post> postRepo,
        IBaseRepository<Category> categoryRepo,
        IAppPathProvider paths,
        IFileStorage fileStorage,
        ConfigService conf,
        CommonService commonService,
        IBackgroundTaskQueue backgroundTaskQueue,
        IServiceScopeFactory scopeFactory,
        ILogger<PostService> logger) {
        _postRepo = postRepo;
        _categoryRepo = categoryRepo;
        _paths = paths;
        _fileStorage = fileStorage;
        _conf = conf;
        _commonService = commonService;
        _backgroundTaskQueue = backgroundTaskQueue;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    /// <summary>
    /// Kiểm tra slug có dùng được hay không
    /// </summary>
    public async Task<bool> CheckSlugAvailable(string slug) {
        return !await _postRepo.Select.AnyAsync(a => a.Slug == slug);
    }

    public async Task<Post?> GetById(string id) {
        // Khi lấy bài viết, phân tích địa chỉ hình ảnh trong markdown, thêm địa chỉ đầy đủ rồi trả về cho frontend
        var post = await _postRepo.Where(a => a.Id == id).Include(a => a.Category).FirstAsync();
        if (post != null) post.Content = MdImageLinkConvert(post, true);

        return post;
    }

    public async Task<Post?> GetBySlug(string slug) {
        var post = await _postRepo.Where(a => a.Slug == slug).FirstAsync();
        if (post != null) post.Content = MdImageLinkConvert(post, true);
        return post;
    }

    public async Task<int> Delete(string id) {
        // todo Khi xóa bài viết, tiện thể xóa luôn tài nguyên hình ảnh tương ứng
        return await _postRepo.DeleteAsync(a => a.Id == id);
    }

    public async Task<Post> InsertOrUpdateAsync(Post post) {
        var postId = post.Id;
        // Nếu là bài viết mới thì lưu vào cơ sở dữ liệu trước
        if (await _postRepo.Where(a => a.Id == postId).CountAsync() == 0) {
            post = await _postRepo.InsertAsync(post);
        }

        if (ContainsExternalImages(post)) {
            var postIdForJob = post.Id;
            await _backgroundTaskQueue.QueueBackgroundWorkItemAsync(async cancellationToken => {
                using var scope = _scopeFactory.CreateScope();
                var service = scope.ServiceProvider.GetRequiredService<PostService>();
                await service.LocalizeExternalImagesAsync(postIdForJob, cancellationToken);
            });
        }

        // Khi sửa bài viết, thay địa chỉ hình ảnh của site này trong markdown thành đường dẫn tương đối rồi mới lưu
        post.Content = MdImageLinkConvert(post, false);
        // todo Khi sửa bài viết, đồng thời phải sinh lại cấu trúc phân cấp danh mục


        // Xử lý xong nội dung thì cập nhật thêm một lần
        await _postRepo.UpdateAsync(post);
        return post;
    }

    public async Task LocalizeExternalImagesAsync(string postId, CancellationToken cancellationToken = default) {
        cancellationToken.ThrowIfCancellationRequested();
        if (string.IsNullOrWhiteSpace(postId)) return;

        var post = await _postRepo.Where(a => a.Id == postId).FirstAsync();
        if (post == null) return;
        if (!ContainsExternalImages(post)) return;

        post.Content = await MdExternalUrlDownloadAsync(post);
        post.Content = MdImageLinkConvert(post, false);
        post.LastUpdateTime = DateTime.Now;

        cancellationToken.ThrowIfCancellationRequested();
        await _postRepo.UpdateAsync(post);
    }

    /// <summary>
    /// Tải ảnh lên cho bài viết chỉ định
    /// </summary>
    public async Task<string> UploadImage(Post post, Stream fileStream, string originalFileName) {
        await _fileStorage.EnsureDirectoryAsync(Path.Combine("media", "blog", post.Id));

        // Sinh trực tiếp tên tệp duy nhất, không giữ lại tên tệp gốc nữa.——2023-6-5 21:21:46
        var filename = GuidUtils.GuidTo16String() + Path.GetExtension(originalFileName);
        var fileRelativePath = Path.Combine("media", "blog", post.Id, filename);
        await _fileStorage.SaveAsync(fileRelativePath, fileStream);
        return $"{Host}/{fileRelativePath.Replace('\\', '/')}";
    }

    /// <summary>
    /// Lấy tài nguyên hình ảnh của bài viết chỉ định
    /// </summary>
    public List<string> GetImages(Post post) {
        var data = new List<string>();
        var files = _fileStorage.ListFilesAsync(Path.Combine("media", "blog", post.Id)).GetAwaiter().GetResult();
        foreach (var file in files) {
            data.Add($"{Host}/media/blog/{post.Id}/{file}");
        }

        return data;
    }

    public async Task<IPagedList<Post>> GetPagedList(PostQueryParameters param, bool adminMode = false) {
        var querySet = _postRepo.Select;

        // Lọc theo trạng thái xuất bản
        // Chỉ quản trị viên mới được lọc theo trạng thái xuất bản
        if (param.IsPublish != null && adminMode) {
            querySet = _postRepo.Select.Where(a => a.IsPublish == param.IsPublish);
        }

        if (!adminMode) {
            querySet = _postRepo.Select.Where(a => a.IsPublish);
        }

        // Lọc theo trạng thái
        if (!string.IsNullOrWhiteSpace(param.Status)) {
            querySet = querySet.Where(a => a.Status == param.Status);
        }

        // Lọc theo danh mục
        if (param.CategoryId != 0) {
            querySet = querySet.Where(a => a.CategoryId == param.CategoryId);
        }

        // Lọc theo từ khóa
        if (!string.IsNullOrWhiteSpace(param.Search)) {
            querySet = querySet.Where(a => a.Title.Contains(param.Search));
        }

        // Sắp xếp
        if (!string.IsNullOrWhiteSpace(param.SortBy)) {
            // Có sắp xếp tăng dần hay không
            var isAscending = !param.SortBy.StartsWith("-");
            var orderByProperty = param.SortBy.Trim('-');

            querySet = querySet.OrderByPropertyName(orderByProperty, isAscending);
        }

        IPagedList<Post> pagedList = new StaticPagedList<Post>(
            await querySet.Page(param.Page, param.PageSize).Include(a => a.Category).ToListAsync(),
            param.Page, param.PageSize, Convert.ToInt32(await querySet.CountAsync())
        );
        return pagedList;
    }

    public static string GetContentHtml(Post post) {
        // todo Nghiên cứu render Markdown ở backend (PS: tuy render ở frontend có nhiều thư viện hơn, hiệu ứng tốt hơn, nhưng render ở backend sẽ không có cảm giác đứt gãy)
        // Một số tài liệu tham khảo cho phần này:
        // - Về kiểu dáng render MarkDown ở frontend: https://blog.csdn.net/sprintline/article/details/122849907
        // - https://github.com/showdownjs/showdown
        var pipeline = new MarkdownPipelineBuilder()
            .UseAdvancedExtensions()
            .UseBootstrap5()
            .Build();
        return Markdown.ToHtml(post.Content ?? "", pipeline);
    }

    /// <summary>
    /// Khởi tạo thư mục tài nguyên của bài viết blog
    /// </summary>
    /// <param name="post"></param>
    /// <returns></returns>
    private string InitPostMediaDir(Post post) {
        var blogMediaDir = Path.Combine(_paths.WebRootPath, "media", "blog");
        var postMediaDir = Path.Combine(_paths.WebRootPath, "media", "blog", post.Id);
        if (!Directory.Exists(blogMediaDir)) Directory.CreateDirectory(blogMediaDir);
        if (!Directory.Exists(postMediaDir)) Directory.CreateDirectory(postMediaDir);

        return postMediaDir;
    }

    /// <summary>
    /// Chuyển đổi liên kết hình ảnh trong Markdown
    /// <para>Hỗ trợ thêm hoặc bỏ tiền tố URL của hình ảnh trong Markdown</para>
    /// </summary>
    /// <param name="post"></param>
    /// <param name="isAddPrefix">Có thêm tiền tố URL đầy đủ của site này hay không</param>
    /// <returns></returns>
    private string MdImageLinkConvert(Post post, bool isAddPrefix = true) {
        if (post.Content == null) return string.Empty;
        var document = Markdown.Parse(post.Content);

        foreach (var node in document.AsEnumerable()) {
            if (node is not ParagraphBlock { Inline: { } } paragraphBlock) continue;
            foreach (var inline in paragraphBlock.Inline) {
                if (inline is not LinkInline { IsImage: true } linkInline) continue;

                var imgUrl = linkInline.Url;
                if (imgUrl == null) continue;

                // Đã có tiền tố Host, bỏ qua
                if (isAddPrefix && imgUrl.StartsWith(Host)) continue;

                // Đặt liên kết đầy đủ
                if (isAddPrefix) {
                    if (imgUrl.StartsWith("http")) continue;
                    linkInline.Url = $"{Host}/media/blog/{post.Id}/{imgUrl}";
                }
                // Đặt thành liên kết tương đối
                else {
                    if (imgUrl.StartsWith(Host)) {
                        linkInline.Url = Path.GetFileName(imgUrl);
                    }
                }
            }
        }

        using var writer = new StringWriter();
        var render = new NormalizeRenderer(writer);
        render.Render(document);
        return writer.ToString();
    }

    private bool ContainsExternalImages(Post post) {
        if (post.Content == null) return false;

        var document = Markdown.Parse(post.Content);
        foreach (var node in document.AsEnumerable()) {
            if (node is not ParagraphBlock { Inline: { } } paragraphBlock) continue;
            foreach (var inline in paragraphBlock.Inline) {
                if (inline is not LinkInline { IsImage: true } linkInline) continue;

                var imgUrl = linkInline.Url;
                if (imgUrl == null) continue;
                if (imgUrl.StartsWith(Host)) continue;
                if (imgUrl.StartsWith("./") || imgUrl.StartsWith("../") || imgUrl.StartsWith("/")) continue;
                if (!imgUrl.StartsWith("http://") && !imgUrl.StartsWith("https://")) continue;

                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Tải xuống hình ảnh bên ngoài trong Markdown
    /// <para>Nếu Markdown chứa URL hình ảnh bên ngoài thì tải về cục bộ và thay thế URL</para>
    /// </summary>
    /// <param name="post"></param>
    /// <returns></returns>
    private async Task<string> MdExternalUrlDownloadAsync(Post post) {
        if (post.Content == null) return string.Empty;

        // Phải khởi tạo thư mục trước
        InitPostMediaDir(post);

        var document = Markdown.Parse(post.Content);
        foreach (var node in document.AsEnumerable()) {
            if (node is not ParagraphBlock { Inline: { } } paragraphBlock) continue;
            foreach (var inline in paragraphBlock.Inline) {
                if (inline is not LinkInline { IsImage: true } linkInline) continue;

                var imgUrl = linkInline.Url;
                // Bỏ qua liên kết rỗng
                if (imgUrl == null) continue;
                // Bỏ qua hình ảnh có địa chỉ của site này
                if (imgUrl.StartsWith(Host)) continue;
                // Bỏ qua đường dẫn tương đối và đường dẫn cục bộ
                if (imgUrl.StartsWith("./") || imgUrl.StartsWith("../") || imgUrl.StartsWith("/")) continue;
                // Bỏ qua liên kết không dùng giao thức HTTP (như file:// v.v.)
                if (!imgUrl.StartsWith("http://") && !imgUrl.StartsWith("https://")) continue;

                // Tải xuống hình ảnh
                _logger.LogDebug("Bài viết: {Title}, tải xuống hình ảnh: {Url}", post.Title, imgUrl);
                var savePath = Path.Combine(_paths.WebRootPath, "media", "blog", post.Id!);
                var fileName = await _commonService.DownloadFileAsync(imgUrl, savePath);
                linkInline.Url = fileName;
            }
        }

        await using var writer = new StringWriter();
        var render = new NormalizeRenderer(writer);
        render.Render(document);
        return writer.ToString();
    }

    /// <summary>
    /// Lấy đề xuất bài viết liên quan (thứ tự ngẫu nhiên)
    /// </summary>
    public async Task<List<Post>> GetRelatedPosts(Post currentPost, int count = 5) {
        var relatedPosts = new List<Post>();

        // 1. Ưu tiên đề xuất bài viết cùng danh mục (thứ tự ngẫu nhiên)
        if (currentPost.CategoryId > 0) {
            var sameCategoryPosts = await _postRepo
                .Where(p => p.IsPublish && p.Id != currentPost.Id && p.CategoryId == currentPost.CategoryId)
                .ToListAsync();

            // Xáo trộn ngẫu nhiên các bài viết cùng danh mục
            var randomSameCategoryPosts = sameCategoryPosts
                .OrderBy(x => Random.Shared.Next())
                .Take(count)
                .ToList();
            relatedPosts.AddRange(randomSameCategoryPosts);
        }

        // 2. Nếu bài viết cùng danh mục không đủ, bổ sung từ các danh mục khác (thứ tự ngẫu nhiên)
        if (relatedPosts.Count < count) {
            var remainingCount = count - relatedPosts.Count;
            var excludeIds = relatedPosts.Select(r => r.Id).ToList();
            excludeIds.Add(currentPost.Id); // Loại trừ bài viết hiện tại

            var otherPosts = await _postRepo
                .Where(p => p.IsPublish && !excludeIds.Contains(p.Id))
                .ToListAsync();

            // Xáo trộn ngẫu nhiên các bài viết khác
            var randomOtherPosts = otherPosts
                .OrderBy(x => Random.Shared.Next())
                .Take(remainingCount)
                .ToList();
            relatedPosts.AddRange(randomOtherPosts);
        }

        return relatedPosts.Take(count).ToList();
    }

    /// <summary>
    /// Lấy bài viết phổ biến
    /// </summary>
    public async Task<List<Post>> GetPopularPosts(int count = 10) {
        return await _postRepo
            .Where(p => p.IsPublish)
            .OrderByDescending(p => p.LastUpdateTime)
            .Take(count)
            .ToListAsync();
    }

    /// <summary>
    /// Lấy bài viết mới nhất
    /// </summary>
    public async Task<List<Post>> GetLatestPosts(int count = 10) {
        return await _postRepo
            .Where(p => p.IsPublish)
            .OrderByDescending(p => p.CreationTime)
            .Take(count)
            .ToListAsync();
    }
}
