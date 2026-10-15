using Microsoft.AspNetCore.Mvc;
using StarBlog.Api.Extensions;
using StarBlog.Application.Services;

namespace StarBlog.Api.Apis.Common; 

/// <summary>
/// Giao diện trang
/// </summary>
[ApiController]
[Route("Api/[controller]")]
[ApiExplorerSettings(GroupName = ApiGroups.Common)]
public class ThemeController : ControllerBase {
    private readonly ThemeService _themeService;

    public ThemeController(ThemeService themeService) {
        _themeService = themeService;
    }

    [HttpGet]
    public List<Theme> GetAll() {
        return _themeService.Themes;
    }
}