using FreeSql.DataAnnotations;

namespace StarBlog.Data.Models;

public enum OutboxStatus {
    Pending = 0,
    Processing = 1,
    Succeeded = 2,
    Dead = 3,
}

/// <summary>
/// Outbox Thông điệp (tác vụ bất đồng bộ đáng tin cậy)
/// <para>Lưu trong cơ sở dữ liệu chính (SQL Server), do FreeSQL CodeFirst tự động tạo bảng/đồng bộ cấu trúc.</para>
/// </summary>
[Table(Name = "outbox_message")]
public class OutboxMessage {
    /// <summary>
    /// Khóa chính tự tăng (dùng để lấy theo trang tuần tự/xếp hàng) 
    /// </summary>
    [Column(IsIdentity = true, IsPrimary = true)]
    public long Id { get; set; }

    /// <summary>
    /// Loại tác vụ
    /// <para>Ví dụ: email.send</para>
    /// </summary>
    public string Type { get; set; } = null!;

    /// <summary>
    /// Khóa idempotent (tùy chọn)
    /// <para>Dùng để tránh cùng một sự kiện nghiệp vụ bị đưa vào hàng đợi lặp lại (ví dụ: reply:{replyId}).</para>
    /// </summary>
    public string? DedupKey { get; set; }

    /// <summary>
    /// Payload tác vụ (chuỗi JSON) 
    /// </summary>
    [Column(StringLength = -1)]
    public string Payload { get; set; } = null!;

    /// <summary>
    /// Trạng thái hiện tại
    /// </summary>
    public OutboxStatus Status { get; set; } = OutboxStatus.Pending;

    /// <summary>
    /// Số lần đã thử
    /// </summary>
    public int Attempt { get; set; } = 0;

    /// <summary>
    /// Số lần thử tối đa
    /// <para>Vượt quá số lần này sẽ bị đánh dấu là Dead.</para>
    /// </summary>
    public int MaxAttempts { get; set; } = 5;

    /// <summary>
    /// Thời điểm có thể thực thi lần tới (dùng cho thử lại có backoff)
    /// </summary>
    public DateTime NextAttemptAt { get; set; } = DateTime.Now;

    /// <summary>
    /// Khóa lease: thời điểm khóa hết hạn (tránh xử lý trùng khi nhiều instance/Worker đồng thời)
    /// </summary>
    public DateTime? LockedUntil { get; set; }

    /// <summary>
    /// Khóa lease: định danh bên giữ khóa (có thể dùng để điều tra và truy vết)
    /// </summary>
    public string? LockedBy { get; set; }

    /// <summary>
    /// Lý do thất bại lần cuối (có thể cắt bớt)
    /// </summary>
    [Column(StringLength = 4000)]
    public string? LastError { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}
