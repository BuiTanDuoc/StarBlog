using CodeLab.Share.ViewModels.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StarBlog.Data.Models;
using StarBlog.Content.Extensions;
using StarBlog.Api.Extensions;
using StarBlog.Application.Services;
using StarBlog.Application.ViewModels.Auth;

namespace StarBlog.Api.Apis;

/// <summary>
/// Xác thực
/// </summary>
[ApiController]
[Route("Api/[controller]")]
[ApiExplorerSettings(GroupName = ApiGroups.Auth)]
public class AuthController : ControllerBase {
    private readonly AuthService _authService;

    public AuthController(AuthService authService) {
        _authService = authService;
    }

    /// <summary>
    /// Đăng nhập
    /// </summary>
    /// <param name="loginUser"></param>
    /// <returns></returns>
    [HttpPost]
    [Route("")]
    [Route("[action]")]
    [ProducesResponseType(typeof(ApiResponse<LoginToken>), StatusCodes.Status200OK)]
    public async Task<ApiResponse> Login(LoginUser loginUser) {
        var user = await _authService.GetUserByName(loginUser.Username);
        if (user == null) return ApiResponse.Unauthorized("Tên đăng nhập hoặc mật khẩu không đúng");
        if (loginUser.Password.ToSHA256() != user.Password) return ApiResponse.Unauthorized("Tên đăng nhập hoặc mật khẩu không đúng");
        return ApiResponse.Ok(_authService.GenerateLoginToken(user));
    }

    /// <summary>
    /// Lấy thông tin người dùng hiện tại
    /// </summary>
    /// <returns></returns>
    [Authorize]
    [HttpGet]
    [Route("")]
    [Route("[action]")]
    public ApiResponse<User> GetUser() {
        var user = _authService.GetUser(User);
        if (user == null) return ApiResponse.NotFound("Không tìm thấy hồ sơ người dùng");
        return new ApiResponse<User>(user);
    }
}
