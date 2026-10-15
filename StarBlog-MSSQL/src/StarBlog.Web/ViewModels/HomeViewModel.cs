using StarBlog.Data.Models;

namespace StarBlog.Web.ViewModels;

public class HomeViewModel {
    /// <summary>
    /// Ảnh ngẫu nhiên
    /// </summary>
    public Photo? RandomPhoto { get; set; }

    /// <summary>
    /// Có hiển thị hay không Chart
    /// </summary>
    public bool ChartVisible { get; set; } = true;

    /// <summary>
    /// Có hiển thị ảnh ngẫu nhiên hay không
    /// </summary>
    public bool RandomPhotoVisible { get; set; } = false;

    /// <summary>
    /// Blog được ghim
    /// </summary>
    public Post? TopPost { get; set; }

    /// <summary>
    /// Blog nổi bật
    /// </summary>
    public List<Post> FeaturedPosts { get; set; } = new();

    /// <summary>
    /// Ảnh nổi bật, về nguyên tắc chỉ được ba ảnh
    /// </summary>
    public List<Photo> FeaturedPhotos { get; set; } = new();

    /// <summary>
    /// Danh mục nổi bật, về nguyên tắc là ba
    /// </summary>
    public List<FeaturedCategory> FeaturedCategories { get; set; } = new();

    /// <summary>
    /// Liên kết bạn bè
    /// </summary>
    public List<Link> Links { get; set; } = new();
}