namespace StarBlog.Web.ViewModels.Links; 

public class LinkCreationDto {
    /// <summary>
    /// Tên website
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Giới thiệu
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// URL
    /// </summary>
    public string Url { get; set; }

    /// <summary>
    /// Có hiển thị hay không
    /// </summary>
    public bool Visible { get; set; }
}