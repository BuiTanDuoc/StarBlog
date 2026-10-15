using System.Text.Encodings.Web;
using System.Text.Json;
using StarBlog.Data.Models;
using StarBlog.Content.Extensions.Markdown;

namespace StarBlog.Web.ViewModels;

public class PostViewModel {
    public string Id { get; set; }
    public string Title { get; set; }
    public string Summary { get; set; }
    public string Content { get; set; }
    public string ContentHtml { get; set; }
    public string Path { get; set; }
    public string? Url { get; set; }
    public DateTime CreationTime { get; set; }
    public DateTime LastUpdateTime { get; set; }
    public Category Category { get; set; }
    public List<Category> Categories { get; set; }
    public List<TocNode>? TocNodes { get; set; }

    /// <summary>
    /// Ngôn ngữ hiện tại (null hoặc "zh" nghĩa là nguyên văn tiếng Trung)
    /// </summary>
    public string? CurrentLanguage { get; set; }

    /// <summary>
    /// Danh sách ngôn ngữ dịch khả dụng
    /// </summary>
    public List<string> AvailableLanguages { get; set; } = new();

    /// <summary>
    /// Hiện có đang hiển thị bản dịch hay không
    /// </summary>
    public bool IsTranslation => !string.IsNullOrEmpty(CurrentLanguage) && CurrentLanguage != "zh";

    public string TocNodesJson => JsonSerializer.Serialize(
        TocNodes,
        new JsonSerializerOptions {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            PropertyNameCaseInsensitive = false,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        }
    );
}
