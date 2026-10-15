using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Net;

namespace StarBlog.Api.Middlewares;

/// <summary>
/// Middleware xác thực truy cập Swagger
/// Cung cấp kiểm soát truy cập cho Swagger UI và tài liệu API
/// </summary>
public class SwaggerAuthMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<SwaggerAuthMiddleware> _logger;

    public SwaggerAuthMiddleware(RequestDelegate next, ILogger<SwaggerAuthMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    /// <summary>
    /// Xử lý HTTP request, kiểm tra quyền truy cập các đường dẫn liên quan đến Swagger
    /// </summary>
    /// <param name="context">HTTP context</param>
    /// <returns>Tác vụ bất đồng bộ</returns>
    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value?.ToLower();
        
        // Kiểm tra xem có phải đường dẫn liên quan đến Swagger không
        if (IsSwaggerPath(path))
        {
            // Kiểm tra người dùng đã được xác thực qua JWT chưa
            if (!context.User.Identity?.IsAuthenticated == true)
            {
                _logger.LogWarning("Truy cập trái phép vào Swagger UI: {Path} from {RemoteIpAddress}", 
                    context.Request.Path, context.Connection.RemoteIpAddress);
                
                await HandleUnauthorizedAccess(context);
                return;
            }
            
            _logger.LogInformation("Người dùng đã xác thực truy cập Swagger: {User} -> {Path}", 
                context.User.Identity.Name, context.Request.Path);
        }

        await _next(context);
    }

    /// <summary>
    /// Xác định có phải đường dẫn liên quan đến Swagger không
    /// </summary>
    /// <param name="path">Đường dẫn yêu cầu</param>
    /// <returns>Có phải đường dẫn Swagger không</returns>
    private static bool IsSwaggerPath(string? path)
    {
        if (string.IsNullOrEmpty(path)) return false;
        
        return path.StartsWith("/swagger") || 
               path.StartsWith("/api-docs");
    }

    /// <summary>
    /// Xử lý truy cập trái phép
    /// </summary>
    /// <param name="context">HTTP context</param>
    /// <returns>Tác vụ bất đồng bộ</returns>
    private static async Task HandleUnauthorizedAccess(HttpContext context)
    {
        context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
        context.Response.ContentType = "application/json; charset=utf-8";
        
        var response = new
        {
            error = "Unauthorized",
            message = "Truy cập Swagger UI cần xác thực bằng JWT token hợp lệ",
            details = "Vui lòng lấy JWT token qua API /Api/Auth/login trước, sau đó thêm vào request header 'Authorization: Bearer {token}'",
            timestamp = DateTime.UtcNow
        };
        
        await context.Response.WriteAsJsonAsync(response);
    }
}

/// <summary>
/// Extension method của middleware xác thực Swagger
/// </summary>
public static class SwaggerAuthMiddlewareExtensions
{
    /// <summary>
    /// Thêm middleware xác thực Swagger
    /// </summary>
    /// <param name="builder">Application builder</param>
    /// <returns>Application builder</returns>
    public static IApplicationBuilder UseSwaggerAuth(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<SwaggerAuthMiddleware>();
    }
}