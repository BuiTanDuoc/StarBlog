using Microsoft.Extensions.Options;
using StarBlog.Application.Models.Config;

namespace StarBlog.Application.Services;

/// <summary>
/// Dịch vụ giám sát, dùng để quản lý cấu hình phân tích và giám sát website
/// </summary>
public class MonitoringService
{
    private readonly Monitoring _config;

    public MonitoringService(IOptions<Monitoring> options)
    {
        _config = options.Value;
    }

    /// <summary>
    /// Lấy cấu hình Umami
    /// </summary>
    public UmamiConfig UmamiConfig => _config.Umami;

    /// <summary>
    /// Lấy cấu hình Clarity
    /// </summary>
    public ClarityConfig ClarityConfig => _config.Clarity;

    /// <summary>
    /// Kiểm tra xem có bật dịch vụ giám sát nào không
    /// </summary>
    public bool IsAnyMonitoringEnabled => _config.Umami.Enabled || _config.Clarity.Enabled;

    /// <summary>
    /// Kiểm tra xem đã bật Umami
    /// </summary>
    public bool IsUmamiEnabled => _config.Umami.Enabled && !string.IsNullOrEmpty(_config.Umami.WebsiteId);

    /// <summary>
    /// Kiểm tra xem đã bật Clarity
    /// </summary>
    public bool IsClarityEnabled => _config.Clarity.Enabled && !string.IsNullOrEmpty(_config.Clarity.ProjectId);
}