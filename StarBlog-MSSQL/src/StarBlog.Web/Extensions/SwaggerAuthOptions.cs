using StarBlog.Web.Middlewares;

namespace StarBlog.Web.Extensions;

/// <summary>
/// Tùy chọn cấu hình xác thực Swagger
/// </summary>
public class SwaggerAuthOptions {
    /// <summary>
    /// Có bật xác thực truy cập Swagger hay không, mặc định là true
    /// </summary>
    public bool RequireAuthentication { get; set; } = true;

    /// <summary>
    /// Có chỉ bật xác thực ở môi trường production hay không, mặc định là false (bật ở mọi môi trường)
    /// </summary>
    public bool OnlyInProduction { get; set; } = false;

    /// <summary>
    /// Danh sách vai trò được phép, để trống thì chỉ cần đã xác thực
    /// </summary>
    public string[] AllowedRoles { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Thông báo phản hồi tùy chỉnh khi chưa xác thực
    /// </summary>
    public string? UnauthorizedMessage { get; set; }
}

/// <summary>
/// Extension method cấu hình xác thực Swagger
/// </summary>
public static class SwaggerAuthConfigExtensions {
    /// <summary>
    /// Cấu hình tùy chọn xác thực Swagger
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="configureOptions">Delegate cấu hình tùy chọn</param>
    /// <returns>Service collection</returns>
    public static IServiceCollection ConfigureSwaggerAuth(this IServiceCollection services,
        Action<SwaggerAuthOptions> configureOptions) {
        services.Configure(configureOptions);
        return services;
    }

    /// <summary>
    /// Dùng middleware xác thực Swagger (kèm tùy chọn cấu hình)
    /// </summary>
    /// <param name="app">Application builder</param>
    /// <param name="options">Tùy chọn xác thực</param>
    /// <returns>Application builder</returns>
    public static IApplicationBuilder UseSwaggerAuth(this IApplicationBuilder app, SwaggerAuthOptions? options = null) {
        if (options != null) {
            // Nếu đã đặt chỉ bật ở production mà môi trường hiện tại không phải production thì bỏ qua xác thực
            if (options.OnlyInProduction) {
                var env = app.ApplicationServices.GetRequiredService<IWebHostEnvironment>();
                if (!env.IsProduction()) {
                    return app;
                }
            }

            // Nếu đã tắt xác thực thì bỏ qua xác thực
            if (!options.RequireAuthentication) {
                return app;
            }
        }

        return app.UseMiddleware<SwaggerAuthMiddleware>();
    }
}