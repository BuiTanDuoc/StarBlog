using FreeSql.DataAnnotations;

namespace StarBlog.Data.Models;

/// <summary>
/// Ảnh nổi bật
/// </summary>
[Table(Name = "featured_photo", OldName = "FeaturedPhoto")]
public class FeaturedPhoto {
    [Column(IsIdentity = true, IsPrimary = true)]
    public int Id { get; set; }

    public string PhotoId { get; set; }
    public Photo Photo { get; set; }
}