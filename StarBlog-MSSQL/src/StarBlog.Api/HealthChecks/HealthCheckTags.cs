namespace StarBlog.Api.HealthChecks;

/// <summary>
/// Health Check Tag:
/// - Live: probe còn sống (không kiểm tra phụ thuộc bên ngoài)
/// - Ready: probe sẵn sàng (kiểm tra các phụ thuộc quan trọng)
/// </summary>
public static class HealthCheckTags {
    public const string Live = "live";
    public const string Ready = "ready";
}

