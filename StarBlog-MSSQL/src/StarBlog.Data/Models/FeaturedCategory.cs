using FreeSql.DataAnnotations;

namespace StarBlog.Data.Models;

/// <summary>
/// Danh mục nổi bật
/// </summary>
[Table(Name = "featured_category", OldName = "FeaturedCategory")]
public class FeaturedCategory {
    [Column(IsIdentity = true, IsPrimary = true)]
    public int Id { get; set; }

    public int CategoryId { get; set; }
    public Category Category { get; set; }

    /// <summary>
    /// Tên nổi bật được định nghĩa lại
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Giải thích về danh mục nổi bật
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Biểu tượng
    /// <list type="number">
    ///     <listheader>Ví dụ</listheader>
    ///     <item>fa-solid fa-c</item>
    ///     <item>fa-brands fa-python</item>
    ///     <item>fa-brands fa-android</item>
    /// </list>
    /// </summary>
    public string IconCssClass { get; set; }
}