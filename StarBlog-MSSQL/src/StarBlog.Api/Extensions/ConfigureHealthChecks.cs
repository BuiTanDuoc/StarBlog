using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using StarBlog.Api.HealthChecks;

namespace StarBlog.Api.Extensions;

public static class ConfigureHealthChecks {
    /// <summary>
    /// Đăng ký health check (Health Checks) cho StarBlog.Api.
    /// - Liveness: chỉ dùng để xác định tiến trình còn "sống" hay không (không phụ thuộc tài nguyên bên ngoài)
    /// - Readiness: dùng để xác định dịch vụ đã "sẵn sàng" chưa (sẽ kiểm tra các phụ thuộc quan trọng có khả dụng không, ví dụ cơ sở dữ liệu)
    /// </summary>
    public static void AddStarBlogHealthChecks(this IServiceCollection services) {
        services
            .AddHealthChecks()
            .AddCheck<LiveHealthCheck>(
                name: "live",
                tags: new[] { HealthCheckTags.Live }
            )
            .AddCheck<LogDbHealthCheck>(
                name: "sqlserver-log",
                tags: new[] { HealthCheckTags.Ready }
            )
            .AddCheck<FreeSqlHealthCheck>(
                name: "freesql",
                tags: new[] { HealthCheckTags.Ready }
            );
    }

    /// <summary>
    /// Ánh xạ các endpoint health check:
    /// - /health/live: probe còn sống
    /// - /health/ready: probe sẵn sàng
    /// - /health, /healthz: probe tổng hợp (alias tương thích)
    /// </summary>
    public static WebApplication MapStarBlogHealthChecks(this WebApplication app) {
        // HealthChecks: ba loại probe phổ biến nhất (và alias tương thích /healthz)
        app.MapHealthChecks(
                "/health/live",
                CreateHealthCheckOptions(r => r.Tags.Contains(HealthCheckTags.Live))
            )
            .AllowAnonymous();

        app.MapHealthChecks(
                "/health/ready",
                CreateHealthCheckOptions(r => r.Tags.Contains(HealthCheckTags.Ready))
            )
            .AllowAnonymous();

        app.MapHealthChecks(
                "/health",
                CreateHealthCheckOptions(_ => true)
            )
            .AllowAnonymous();

        app.MapHealthChecks(
                "/healthz",
                CreateHealthCheckOptions(_ => true)
            )
            .AllowAnonymous();

        return app;
    }

    /// <summary>
    /// HealthCheckOptions thống nhất, xuất JSON và trả về mã trạng thái HTTP theo tình trạng sức khỏe.
    /// </summary>
    public static HealthCheckOptions CreateHealthCheckOptions(Func<HealthCheckRegistration, bool> predicate) {
        return new HealthCheckOptions {
            Predicate = predicate,
            AllowCachingResponses = false,
            ResultStatusCodes = {
                [HealthStatus.Healthy] = StatusCodes.Status200OK,
                [HealthStatus.Degraded] = StatusCodes.Status200OK,
                [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
            },
            ResponseWriter = WriteResponseAsync
        };
    }

    private static Task WriteResponseAsync(HttpContext context, HealthReport report) {
        context.Response.ContentType = "application/json; charset=utf-8";

        var payload = new {
            status = report.Status.ToString(),
            totalDurationMs = report.TotalDuration.TotalMilliseconds,
            checks = report.Entries.Select(entry => new {
                name = entry.Key,
                status = entry.Value.Status.ToString(),
                durationMs = entry.Value.Duration.TotalMilliseconds,
                description = entry.Value.Description,
                error = entry.Value.Exception?.Message,
                data = entry.Value.Data.Count == 0 ? null : entry.Value.Data
            })
        };

        return context.Response.WriteAsync(JsonSerializer.Serialize(payload));
    }
}
