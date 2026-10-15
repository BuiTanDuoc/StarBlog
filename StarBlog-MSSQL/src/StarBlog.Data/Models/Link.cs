using FreeSql.DataAnnotations;

namespace StarBlog.Data.Models;

/// <summary>
/// Liên kết bạn bè
/// </summary>
public class Link {
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
    /// Có hiển thị hay không
    /// </summary>
    public bool Visible { get; set; }
}