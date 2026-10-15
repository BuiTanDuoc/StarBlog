using System.Text.RegularExpressions;
using StarBlog.Data.Models;

namespace StarBlog.Web.Services;

public class ImageSeoService {
    private readonly ConfigService _configService;

    public ImageSeoService(ConfigService configService) {
        _configService = configService;
    }

    /// <summary>
    /// Thêm thuộc tính alt tối ưu SEO cho hình ảnh trong nội dung Markdown
    /// </summary>
    public string OptimizeImagesInMarkdown(string content, string postTitle) {
        if (string.IsNullOrEmpty(content)) return content;

        // Khớp cú pháp hình ảnh của Markdown: ![alt](url "title")
        var imagePattern = @"!\[([^\]]*)\]\(([^)]+)(?:\s+""([^""]*)"")?\)";
        
        return Regex.Replace(content, imagePattern, match => {
            var altText = match.Groups[1].Value;
            var imageUrl = match.Groups[2].Value;
            var title = match.Groups[3].Value;

            // Nếu chưa có văn bản alt, tạo một alt thân thiện với SEO
            if (string.IsNullOrEmpty(altText)) {
                altText = GenerateAltText(imageUrl, postTitle);
            }

            // Nếu chưa có title thì dùng alt làm title
            if (string.IsNullOrEmpty(title)) {
                title = altText;
            }

            return $"![{altText}]({imageUrl} \"{title}\")";
        });
    }

    /// <summary>
    /// Thêm tối ưu SEO cho hình ảnh trong nội dung HTML
    /// </summary>
    public string OptimizeImagesInHtml(string htmlContent, string postTitle) {
        if (string.IsNullOrEmpty(htmlContent)) return htmlContent;

        // Khớp thẻ img của HTML
        var imgPattern = @"<img\s+([^>]*?)>";
        
        return Regex.Replace(htmlContent, imgPattern, match => {
            var imgTag = match.Value;
            var attributes = match.Groups[1].Value;

            // Kiểm tra xem đã có thuộc tính alt hay chưa
            if (!attributes.Contains("alt=")) {
                var srcMatch = Regex.Match(attributes, @"src=[""']([^""']+)[""']");
                if (srcMatch.Success) {
                    var imageUrl = srcMatch.Groups[1].Value;
                    var altText = GenerateAltText(imageUrl, postTitle);
                    attributes += $" alt=\"{altText}\"";
                }
            }

            // Thêm loading="lazy" để tăng hiệu năng
            if (!attributes.Contains("loading=")) {
                attributes += " loading=\"lazy\"";
            }

            // Thêm decoding="async" để tăng hiệu năng
            if (!attributes.Contains("decoding=")) {
                attributes += " decoding=\"async\"";
            }

            return $"<img {attributes}>";
        });
    }

    /// <summary>
    /// Tạo văn bản alt thân thiện với SEO
    /// </summary>
    private string GenerateAltText(string imageUrl, string postTitle) {
        // Trích xuất tên tệp từ URL
        var fileName = Path.GetFileNameWithoutExtension(imageUrl);
        
        // Làm sạch tên tệp, loại bỏ ký tự đặc biệt
        fileName = Regex.Replace(fileName, @"[^a-zA-Z0-9\u4e00-\u9fa5\s-]", "");
        
        // Nếu tên tệp có ý nghĩa thì dùng nó; nếu không thì dùng tiêu đề bài viết
        if (!string.IsNullOrEmpty(fileName) && fileName.Length > 3 && !IsGuidLike(fileName)) {
            return $"{fileName} - {postTitle}";
        }

        return $"{postTitle} hình ảnh liên quan";
    }

    /// <summary>
    /// Kiểm tra chuỗi có giống GUID hay không
    /// </summary>
    private bool IsGuidLike(string str) {
        return str.Length >= 16 && Regex.IsMatch(str, @"^[a-fA-F0-9-]+$");
    }

    /// <summary>
    /// Lấy tất cả URL hình ảnh trong bài viết
    /// </summary>
    public List<string> ExtractImageUrls(Post post) {
        var imageUrls = new List<string>();
        var baseUrl = GetBaseUrl();

        if (string.IsNullOrEmpty(post.Content)) return imageUrls;

        // Trích xuất hình ảnh từ nội dung Markdown
        var imagePattern = @"!\[([^\]]*)\]\(([^)]+)(?:\s+""([^""]*)"")?\)";
        var matches = Regex.Matches(post.Content, imagePattern);

        foreach (Match match in matches) {
            var imageUrl = match.Groups[2].Value;
            
            // Chuyển thành URL tuyệt đối
            if (!imageUrl.StartsWith("http")) {
                if (imageUrl.StartsWith("/")) {
                    imageUrl = baseUrl + imageUrl;
                } else {
                    imageUrl = $"{baseUrl}/media/blog/{post.Id}/{imageUrl}";
                }
            }

            imageUrls.Add(imageUrl);
        }

        return imageUrls;
    }



    private string GetBaseUrl() {
        var host = _configService["host"];
        return !string.IsNullOrEmpty(host) ? host.TrimEnd('/') : "https://blog.deali.cn";
    }
}
