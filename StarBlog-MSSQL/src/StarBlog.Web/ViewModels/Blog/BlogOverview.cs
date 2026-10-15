namespace StarBlog.Web.ViewModels.Blog;

/// <summary>
/// Tổng quan thông tin blog
/// </summary>
public class BlogOverview {
    /// <summary>
    /// Số lượng bài viết
    /// </summary>
    public long PostsCount { get; set; }

    /// <summary>
    /// Số lượng danh mục
    /// </summary>
    public long CategoriesCount { get; set; }

    /// <summary>
    /// Số lượng ảnh
    /// </summary>
    public long PhotosCount { get; set; }

    /// <summary>
    /// Số lượng bài viết nổi bật
    /// </summary>
    public long FeaturedPostsCount { get; set; }

    /// <summary>
    /// Số lượng danh mục nổi bật
    /// </summary>
    public long FeaturedCategoriesCount { get; set; }

    /// <summary>
    /// Số lượng ảnh nổi bật
    /// </summary>
    public long FeaturedPhotosCount { get; set; }
}