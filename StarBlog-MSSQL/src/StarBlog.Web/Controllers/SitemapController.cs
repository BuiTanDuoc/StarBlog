using System.Text;
using Microsoft.AspNetCore.Mvc;
using StarBlog.Web.Services;

namespace StarBlog.Web.Controllers;

[ApiExplorerSettings(IgnoreApi = true)]
public class SitemapController : Controller {
    private readonly SitemapService _sitemapService;
    private readonly ILogger<SitemapController> _logger;

    public SitemapController(
        SitemapService sitemapService,
        ILogger<SitemapController> logger) {
        _sitemapService = sitemapService;
        _logger = logger;
    }

    [HttpGet("sitemap-index.xml")]
    [ResponseCache(Duration = 3600)] // Cache 1 giờ
    public IActionResult SitemapIndex() {
        try {
            var sitemapIndex = _sitemapService.GenerateSitemapIndex();
            return Content(sitemapIndex, "application/xml", Encoding.UTF8);
        }
        catch (Exception ex) {
            _logger.LogError(ex, "Đã xảy ra lỗi khi tạo chỉ mục sitemap");
            return StatusCode(500);
        }
    }

    [HttpGet("sitemap.xml")]
    [ResponseCache(Duration = 3600)] // Cache 1 giờ
    public async Task<IActionResult> Index() {
        try {
            var sitemap = await _sitemapService.GenerateMainSitemapAsync();
            return Content(sitemap, "application/xml", Encoding.UTF8);
        }
        catch (Exception ex) {
            _logger.LogError(ex, "Đã xảy ra lỗi khi tạo sitemap");
            return StatusCode(500);
        }
    }

    [HttpGet("sitemap-images.xml")]
    [ResponseCache(Duration = 3600)] // Cache 1 giờ
    public async Task<IActionResult> ImageSitemap() {
        try {
            var imageSitemap = await _sitemapService.GenerateImageSitemapAsync();
            return Content(imageSitemap, "application/xml", Encoding.UTF8);
        }
        catch (Exception ex) {
            _logger.LogError(ex, "Đã xảy ra lỗi khi tạo sitemap hình ảnh");
            return StatusCode(500);
        }
    }
}