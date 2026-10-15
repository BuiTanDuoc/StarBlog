namespace StarBlog.Application.ViewModels.Blog;

public class PostUpdateDto {
    public string Id { get; set; }

    /// <summary>
    /// Tiêu đề
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// Đường dẫn bài viết, sau khi thiết lập có thể truy cập bài viết theo dạng sau
    /// <para> http://starblog.com/p/post-slug1 </para>
    /// </summary>
    public string? Slug { get; set; }
    
    /// <summary>
    /// Nhãn bài viết
    /// </summary>
    public string? Status { get; set; }
    
    /// <summary>
    /// Có xuất bản hay không (nếu không xuất bản thì là trạng thái bản nháp)
    /// </summary>
    public bool IsPublish { get; set; }

    /// <summary>
    /// Tóm tắt
    /// </summary>
    public string Summary { get; set; }

    /// <summary>
    /// Nội dung (định dạng markdown)
    /// </summary>
    public string Content { get; set; }

    /// <summary>
    /// ID danh mục
    /// </summary>
    public int CategoryId { get; set; }
}