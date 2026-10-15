using FreeSql.DataAnnotations;

namespace StarBlog.Data.Models;

public class Photo {
    [Column(IsIdentity = false, IsPrimary = true)]
    public string Id { get; set; }

    /// <summary>
    /// Tiêu đề tác phẩm
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// Địa điểm chụp
    /// </summary>
    public string Location { get; set; }

    /// <summary>
    /// Vị trí lưu trữ tệp
    /// </summary>
    public string FilePath { get; set; }

    /// <summary>
    /// Chiều cao
    /// </summary>
    public long Height { get; set; }

    /// <summary>
    /// Chiều rộng
    /// </summary>
    public long Width { get; set; }

    /// <summary>
    /// Thời gian tạo
    /// </summary>
    public DateTime CreateTime { get; set; }
}