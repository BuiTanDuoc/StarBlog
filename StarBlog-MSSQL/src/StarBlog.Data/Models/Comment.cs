using FreeSql.DataAnnotations;

namespace StarBlog.Data.Models;

/// <summary>
/// Bình luận bài viết
/// </summary>
public class Comment : ModelBase {
    [Column(IsIdentity = false, IsPrimary = true)]
    public string Id { get; set; }

    public string? ParentId { get; set; }
    public Comment? Parent { get; set; }
    public List<Comment>? Comments { get; set; }

    public string PostId { get; set; }
    public Post Post { get; set; }

    public string? UserId { get; set; }
    public User? User { get; set; }

    public string? AnonymousUserId { get; set; }
    public AnonymousUser? AnonymousUser { get; set; }

    public string? UserAgent { get; set; }
    public string Content { get; set; }
    public bool Visible { get; set; }

    /// <summary>
    /// Có cần kiểm duyệt hay không
    /// </summary>
    public bool IsNeedAudit { get; set; } = false;

    /// <summary>
    /// Lý do
    /// <para>Nếu xác thực không đạt, có thể kèm theo lý do</para>
    /// </summary>
    public string? Reason { get; set; }
}