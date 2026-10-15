using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace StarBlog.Api.HealthChecks;

/// <summary>
/// Probe còn sống: chỉ cần tiến trình phản hồi được HTTP thì trả về Healthy.
/// Phù hợp để container/load balancer xác định dịch vụ có cần khởi động lại không (không phụ thuộc cơ sở dữ liệu/cache và các tài nguyên bên ngoài khác).
/// </summary>
public sealed class LiveHealthCheck : IHealthCheck {
    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default
    ) {
        return Task.FromResult(HealthCheckResult.Healthy("Service is running."));
    }
}

