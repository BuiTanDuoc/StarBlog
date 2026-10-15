using FreeSql.DataAnnotations;

namespace StarBlog.Data.Models;

/// <summary>
/// Bản ghi yêu cầu trao đổi liên kết bạn bè
/// </summary>
public class LinkExchange {
    [Column(IsIdentity = true, IsPrimary = true)]
    public int Id { get; set; }

    /// <summary>
    /// Tên website
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Giới thiệu
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// URL
    /// </summary>
    public string Url { get; set; }

    /// <summary>
    /// Quản trị viên website
    /// </summary>
    public string WebMaster { get; set; }

    /// <summary>
    /// Email liên hệ
    /// </summary>
    public string Email { get; set; }

    /// <summary>
    /// Đã xác minh hay chưa
    /// <para>Liên kết bạn bè phải được xác minh mới hiển thị trên website</para>
    /// </summary>
    public bool Verified { get; set; } = false;

    /// <summary>
    /// Lý do
    /// <para>Nếu xác thực không đạt, có thể kèm theo lý do</para>
    /// </summary>
    public string? Reason { get; set; }

    /// <summary>
    /// Thời gian đăng ký
    /// </summary>
    public DateTime ApplyTime { get; set; } = DateTime.Now;
}