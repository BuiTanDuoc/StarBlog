namespace StarBlog.Web.Criteria; 

/// <summary>
/// Tham số yêu cầu bài viết blog
/// </summary>
public class PostQueryParameters : QueryParameters {
    /// <summary>
    /// Đã xuất bản hay chưa (chỉ quản trị viên mới có thể truy vấn và quản lý bài viết chưa xuất bản)
    /// </summary>
    public bool? IsPublish { get; set; } = null;

    /// <summary>
    /// Trạng thái bài viết
    /// </summary>
    public string? Status { get; set; }
    
    /// <summary>
    /// ID danh mục
    /// </summary>
    public int CategoryId { get; set; } = 0;

    /// <summary>
    /// Có bao gồm danh mục con hay không
    /// </summary>
    public bool IncludeSubCategory { get; set; } = false;

    /// <summary>
    /// Trường sắp xếp
    /// </summary>
    public new string? SortBy { get; set; } = "-LastUpdateTime";
}