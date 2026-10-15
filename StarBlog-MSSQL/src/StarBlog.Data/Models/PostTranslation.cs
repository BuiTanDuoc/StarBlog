using System.ComponentModel.DataAnnotations;
using FreeSql.DataAnnotations;

namespace StarBlog.Data.Models;

/// <summary>
/// Bản dịch bài viết blog
/// </summary>
public class PostTranslation {
    [Column(IsIdentity = false, IsPrimary = true)]
    public string Id { get; set; }

    /// <summary>
    /// ID bài viết liên kết
    /// </summary>
    public string PostId { get; set; }
    public Post Post { get; set; }

    /// <summary>
    /// Mã ngôn ngữ, ví dụ "en", "ja"
    /// </summary>
    public string Language { get; set; }

    /// <summary>
    /// Tiêu đề sau khi dịch
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// Tóm tắt sau khi dịch
    /// </summary>
    public string? Summary { get; set; }

    /// <summary>
    /// Nội dung Markdown sau khi dịch
    /// </summary>
    [MaxLength(-1)]
    public string? Content { get; set; }

    public DateTime CreationTime { get; set; }
    public DateTime LastUpdateTime { get; set; }
}
