using FreeSql;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace StarBlog.Api.HealthChecks;

/// <summary>
/// Readiness: kiểm tra cơ sở dữ liệu nghiệp vụ FreeSql có khả dụng không.
/// Ở đây dùng câu lệnh thăm dò nhẹ nhất (select 1) để xác nhận kết nối và kênh thực thi hoạt động bình thường.
/// </summary>
public sealed class FreeSqlHealthCheck : IHealthCheck {
    private readonly IFreeSql _freeSql;

    public FreeSqlHealthCheck(IFreeSql freeSql) {
        _freeSql = freeSql;
    }

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default
    ) {
        try {
            _freeSql.Ado.ExecuteScalar("select 1");
            return Task.FromResult(HealthCheckResult.Healthy("FreeSql database is reachable."));
        }
        catch (Exception ex) {
            return Task.FromResult(HealthCheckResult.Unhealthy("FreeSql database check failed.", ex));
        }
    }
}

