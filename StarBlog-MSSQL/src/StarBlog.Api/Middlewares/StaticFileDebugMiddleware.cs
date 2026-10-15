using Microsoft.Extensions.FileProviders;

namespace StarBlog.Api.Middlewares;

/// <summary>
/// Middleware gỡ lỗi tệp tĩnh
/// </summary>
public class StaticFileDebugMiddleware(
    RequestDelegate next,
    ILogger<StaticFileDebugMiddleware> logger,
    IWebHostEnvironment env
) {
    public async Task InvokeAsync(HttpContext context) {
        var path = context.Request.Path.Value;

        // Chỉ ghi nhận các request tệp tĩnh
        if (path != null && (path.StartsWith("/js/") || path.StartsWith("/css/") || path.StartsWith("/lib/"))) {
            var physicalPath = Path.Combine(env.WebRootPath, path.TrimStart('/'));
            var fileExists = File.Exists(physicalPath);

            logger.LogInformation("Static file request: {Path}, Physical: {PhysicalPath}, Exists: {Exists}",
                path, physicalPath, fileExists);

            if (!fileExists) {
                logger.LogWarning("Static file not found: {Path} -> {PhysicalPath}", path, physicalPath);
            }
        }

        await next(context);

        // Ghi nhận trạng thái phản hồi
        if (path != null && (path.StartsWith("/js/") || path.StartsWith("/css/") || path.StartsWith("/lib/"))) {
            logger.LogInformation("Static file response: {Path}, Status: {StatusCode}",
                path, context.Response.StatusCode);
        }
    }
}