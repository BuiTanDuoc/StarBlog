using FreeSql;
using Microsoft.AspNetCore.Mvc;
using StarBlog.Data.Models;
using StarBlog.Content.Extensions;
using StarBlog.Web.Contrib.SiteMessage;
using StarBlog.Web.Extensions;
using StarBlog.Web.Services;
using StarBlog.Web.ViewModels;

namespace StarBlog.Web.Controllers;

public class HomeController : Controller {
    private readonly BlogService _blogService;
    private readonly PhotoService _photoService;
    private readonly CategoryService _categoryService;
    private readonly LinkService _linkService;
    private readonly MessageService _messages;
    private readonly ConfigService _conf;
    private readonly SeoService _seoService;
    private readonly StructuredDataService _structuredDataService;

    public HomeController(BlogService blogService, PhotoService photoService, CategoryService categoryService,
        LinkService linkService,
        MessageService messages, ConfigService conf, SeoService seoService, StructuredDataService structuredDataService) {
        _blogService = blogService;
        _photoService = photoService;
        _categoryService = categoryService;
        _linkService = linkService;
        _messages = messages;
        _conf = conf;
        _seoService = seoService;
        _structuredDataService = structuredDataService;
    }

    public async Task<IActionResult> Index() {
        if (Request.QueryString.HasValue) {
            return BadRequest();
        }

        var vm = new HomeViewModel {
            ChartVisible = _conf["home_chart_visible"] == "true",
            RandomPhotoVisible = _conf["home_random_photo_visible"] == "true",
            RandomPhoto = await _photoService.GetRandomPhoto(),
            TopPost = await _blogService.GetTopOnePost(),
            FeaturedPosts = await _blogService.GetFeaturedPosts(),
            FeaturedPhotos = await _photoService.GetFeaturedPhotos(),
            FeaturedCategories = await _categoryService.GetFeaturedCategories(),
            Links = await _linkService.GetAll()
        };

        if (HttpContext.Request.IsMobileBrowser()) {
            vm.ChartVisible = false;
            vm.RandomPhotoVisible = false;
        }

        // Thiết lập metadata SEO
        ViewData["SeoMetadata"] = _seoService.GetHomeSeoMetadata();

        // Thiết lập dữ liệu có cấu trúc
        var structuredData = new Dictionary<string, string> {
            ["WebSite"] = _structuredDataService.GetWebsiteStructuredData(),
            ["Organization"] = _structuredDataService.GetOrganizationStructuredData(),
            ["Person"] = _structuredDataService.GetPersonStructuredData()
        };
        ViewData["StructuredData"] = structuredData;

        return View(vm);
    }

    [HttpGet]
    public IActionResult Init([FromServices] ConfigService conf) {
        if (conf["is_init"] == "true") {
            _messages.Error("Đã hoàn tất khởi tạo rồi!");
            return RedirectToAction(nameof(Index));
        }

        return View(new InitViewModel {
            Host = conf["host"],
            DefaultRender = conf["default_render"]
        });
    }

    [HttpPost]
    public IActionResult Init([FromServices] IBaseRepository<User> userRepo, InitViewModel vm) {
        if (_conf["is_init"] == "true") {
            _messages.Error("Đã hoàn tất khởi tạo rồi!");
            return RedirectToAction(nameof(Index));
        }

        if (!ModelState.IsValid) return View();

        // Lưu cấu hình
        _conf["host"] = vm.Host;
        _conf["default_render"] = vm.DefaultRender;
        _conf["is_init"] = "true";

        // Tạo người dùng
        // to do Tạm thời lưu mật khẩu dạng văn bản thuần ở đây, sau này sẽ đổi sang lưu mã hóa MD5. Đã xong ngày 2023-5-7
        // todo Dùng mật khẩu băm (hash) có muối (salt) https://www.ais.com/how-to-generate-a-jwt-token-using-net-6/
        userRepo.Insert(new User {
            Id = Guid.NewGuid().ToString(),
            Name = vm.Username,
            Password = vm.Password.ToSHA256()
        });

        _messages.Success("Khởi tạo hoàn tất!");
        return RedirectToAction(nameof(Index));
    }
}
