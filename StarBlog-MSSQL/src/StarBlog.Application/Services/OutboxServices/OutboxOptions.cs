namespace StarBlog.Application.Services.OutboxServices;

/// <summary>
/// Outbox Tham số vận hành
/// <para>Cung cấp giá trị mặc định trước, sau đó có thể ghi đè qua mục Outbox trong appsettings.json.</para>
/// </summary>
public sealed class OutboxOptions {
    /// <summary>
    /// Số tác vụ tối đa nhận mỗi vòng
    /// </summary>
    public int BatchSize { get; set; } = 20;

    /// <summary>
    /// Khoảng thời gian polling khi hàng đợi rảnh
    /// </summary>
    public TimeSpan PollInterval { get; set; } = TimeSpan.FromMilliseconds(500);

    /// <summary>
    /// Thời lượng lease (khóa trong bao lâu sau khi nhận tác vụ)
    /// </summary>
    public TimeSpan LeaseDuration { get; set; } = TimeSpan.FromSeconds(60);

    /// <summary>
    /// Thời lượng backoff tối đa (giới hạn trên của exponential backoff)
    /// </summary>
    public TimeSpan MaxBackoff { get; set; } = TimeSpan.FromMinutes(10);

    /// <summary>
    /// Jitter ngẫu nhiên của backoff (tránh nhiều instance cùng thử lại gây ra hiệu ứng "thundering herd")
    /// </summary>
    public TimeSpan BackoffJitter { get; set; } = TimeSpan.FromMilliseconds(500);

    /// <summary>
    /// Số lần thử lại tối đa mặc định (dùng khi lúc đưa vào hàng đợi không chỉ định)
    /// </summary>
    public int DefaultMaxAttempts { get; set; } = 5;
}
