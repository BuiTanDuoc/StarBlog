using Microsoft.AspNetCore.Mvc;
using StarBlog.Web.Services;
using FreeSql;
using StarBlog.Data.Models;

namespace StarBlog.Web.Controllers;

[ApiExplorerSettings(IgnoreApi = true)]
public class SeoTestController : Controller {
    private readonly SeoService _seoService;
    private readonly StructuredDataService _structuredDataService;
    private readonly ImageSeoService _imageSeoService;
    private readonly SitemapService _sitemapService;
    private readonly IBaseRepository<Post> _postRepo;

    public SeoTestController(
        SeoService seoService,
        StructuredDataService structuredDataService,
        ImageSeoService imageSeoService,
        SitemapService sitemapService,
        IBaseRepository<Post> postRepo) {
        _seoService = seoService;
        _structuredDataService = structuredDataService;
        _imageSeoService = imageSeoService;
        _sitemapService = sitemapService;
        _postRepo = postRepo;
    }

    [HttpGet("seo-test")]
    public async Task<IActionResult> Index() {
        var testResults = new List<string>();

        try {
            // Kiểm thử dịch vụ SEO
            var homeSeo = _seoService.GetHomeSeoMetadata();
            testResults.Add($"✅ Tạo metadata SEO trang chủ thành công: {homeSeo.Title}");

            // Kiểm thử dịch vụ dữ liệu có cấu trúc
            var websiteStructuredData = _structuredDataService.GetWebsiteStructuredData();
            testResults.Add($"✅ Tạo dữ liệu có cấu trúc của website thành công: {websiteStructuredData.Length} ký tự");

            var organizationStructuredData = _structuredDataService.GetOrganizationStructuredData();
            testResults.Add($"✅ Tạo dữ liệu có cấu trúc của tổ chức thành công: {organizationStructuredData.Length} ký tự");

            var personStructuredData = _structuredDataService.GetPersonStructuredData();
            testResults.Add($"✅ Tạo dữ liệu có cấu trúc của cá nhân thành công: {personStructuredData.Length} ký tự");

            // Kiểm thử các chức năng liên quan đến bài viết
            var firstPost = await _postRepo.Where(p => p.IsPublish).FirstAsync();
            if (firstPost != null) {
                // Kiểm thử tối ưu SEO cho hình ảnh
                var testMarkdown = "![Ảnh thử nghiệm](test-image.jpg)";
                var optimizedMarkdown = _imageSeoService.OptimizeImagesInMarkdown(testMarkdown, firstPost.Title);
                testResults.Add($"✅ Tối ưu SEO hình ảnh Markdown thành công: {optimizedMarkdown}");

                var testHtml = "<img src='test.jpg'>";
                var optimizedHtml = _imageSeoService.OptimizeImagesInHtml(testHtml, firstPost.Title);
                testResults.Add($"✅ Tối ưu SEO hình ảnh HTML thành công: {optimizedHtml}");

                // Kiểm thử trích xuất URL hình ảnh
                var imageUrls = _imageSeoService.ExtractImageUrls(firstPost);
                testResults.Add($"✅ Trích xuất URL hình ảnh thành công: tìm thấy {imageUrls.Count} hình ảnh");
            }

            // Kiểm thử dịch vụ Sitemap
            var mainSitemap = await _sitemapService.GenerateMainSitemapAsync();
            testResults.Add($"✅ Tạo sitemap chính thành công: {mainSitemap.Length} ký tự");

            var imageSitemap = await _sitemapService.GenerateImageSitemapAsync();
            testResults.Add($"✅ Tạo sitemap hình ảnh thành công: {imageSitemap.Length} ký tự");

            var sitemapIndex = _sitemapService.GenerateSitemapIndex();
            testResults.Add($"✅ Tạo chỉ mục Sitemap thành công: {sitemapIndex.Length} ký tự");

            testResults.Add("🎉 Tất cả các kiểm thử chức năng SEO đều đạt!");
        }
        catch (Exception ex) {
            testResults.Add($"❌ Kiểm thử thất bại: {ex.Message}");
        }

        ViewBag.TestResults = testResults;
        return View();
    }

    [HttpGet("seo-test/sitemap")]
    public IActionResult TestSitemap() {
        var sitemapUrls = new List<string> {
            "/sitemap.xml",
            "/sitemap-images.xml",
            "/sitemap-index.xml"
        };

        ViewBag.SitemapUrls = sitemapUrls;
        return View();
    }

    [HttpGet("seo-test/meta")]
    public async Task<IActionResult> TestMeta() {
        var posts = await _postRepo.Where(a => a.IsPublish).ToListAsync();
        if (posts.Count == 0) {
            ViewBag.Error = "Không tìm thấy bài viết đã xuất bản";
            return View();
        }
        
        var post = posts[Random.Shared.Next(posts.Count)];

        // Tạo PostViewModel dùng để kiểm thử
        var postViewModel = new StarBlog.Web.ViewModels.PostViewModel {
            Id = post.Id,
            Title = post.Title,
            Summary = post.Summary ?? "Tóm tắt thử nghiệm",
            Content = post.Content ?? "Nội dung thử nghiệm",
            CreationTime = post.CreationTime,
            LastUpdateTime = post.LastUpdateTime,
            Category = post.Category ?? new Category(),
            Categories = new List<Category>()
        };

        // Thiết lập metadata SEO
        ViewData["SeoMetadata"] = _seoService.GetPostSeoMetadata(postViewModel);

        // Thiết lập dữ liệu có cấu trúc
        var structuredData = new Dictionary<string, string> {
            ["BlogPosting"] = _structuredDataService.GetBlogPostingStructuredData(postViewModel),
            ["BreadcrumbList"] = _structuredDataService.GetBreadcrumbStructuredData(postViewModel),
            ["Person"] = _structuredDataService.GetPersonStructuredData()
        };
        ViewData["StructuredData"] = structuredData;

        return View(postViewModel);
    }
}
