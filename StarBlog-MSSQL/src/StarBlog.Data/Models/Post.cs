using System.ComponentModel.DataAnnotations;
using FreeSql.DataAnnotations;

namespace StarBlog.Data.Models;

/// <summary>
/// Bài viết blog
/// </summary>
public class Post {
    [Column(IsIdentity = false, IsPrimary = true)]
    public string Id { get; set; }

    /// <summary>
    /// Tiêu đề
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// Đường dẫn bài viết, sau khi thiết lập có thể truy cập bài viết theo dạng sau
    /// <para> http://starblog.com/p/post-slug1 </para>
    /// </summary>
    [MaxLength(150)]
    public string? Slug { get; set; }
    
    /// <summary>
    /// Nhãn bài viết, trích xuất từ tiền tố tên tệp markdown gốc, dùng để phân biệt trạng thái bài viết, ví dụ như sau
    /// <para>"(Chưa hoàn thành)Ghi chép phát triển blog StarBlog(3): Thiết kế mô hình"</para>
    /// <para>"(Chưa xuất bản)Ghi chép phát triển blog StarBlog(3): Thiết kế mô hình"</para>
    /// </summary>
    public string? Status { get; set; }
    
    // todo Bổ sung chức năng tag

    /// <summary>
    /// Có xuất bản hay không (nếu không xuất bản thì là trạng thái bản nháp)
    /// </summary>
    public bool IsPublish { get; set; }

    /// <summary>
    /// Tóm tắt
    /// </summary>
    public string? Summary { get; set; }

    /// <summary>
    /// Nội dung (định dạng markdown)
    /// </summary>
    [MaxLength(-1)]
    public string? Content { get; set; }

    /// <summary>
    /// Đường dẫn tương đối của blog trước khi nhập
    /// <para>Ví dụ: "Series/Ghi chép phát triển AspNetCore"</para>
    /// </summary>
    public string? Path { get; set; }

    /// <summary>
    /// Thời gian tạo
    /// </summary>
    public DateTime CreationTime { get; set; }

    /// <summary>
    /// Thời gian cập nhật lần cuối
    /// </summary>
    public DateTime LastUpdateTime { get; set; }

    /// <summary>
    /// ID danh mục
    /// </summary>
    public int CategoryId { get; set; }

    /// <summary>
    /// Danh mục
    /// </summary>
    public Category? Category { get; set; }

    /// <summary>
    /// Cấp bậc danh mục của bài viết, nội dung có dạng như `1,2,3`, dùng dấu phẩy ngăn cách các ID danh mục
    /// </summary>
    public string? Categories { get; set; }

    /// <summary>
    /// Danh sách bản dịch của bài viết
    /// </summary>
    public List<PostTranslation>? Translations { get; set; }
}