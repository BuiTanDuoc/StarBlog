namespace StarBlog.Application.ViewModels.Categories; 

public class FeaturedCategoryCreationDto2 {
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
    
    /// <summary>
    /// ID danh mục
    /// </summary>
    public int CategoryId { get; set; }
}