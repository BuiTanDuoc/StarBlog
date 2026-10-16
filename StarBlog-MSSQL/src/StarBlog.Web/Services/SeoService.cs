using StarBlog.Data.Models;
using StarBlog.Web.ViewModels;

namespace StarBlog.Web.Services;

public class SeoService {
    private readonly ConfigService _configService;

    public SeoService(ConfigService configService) {
        _configService = configService;
    }

    public SeoMetadata GetHomeSeoMetadata() {
        var siteName = "StarBlog · Cao thủ vẽ ngôi sao · Phòng thí nghiệm lập trình";
        var description =
            "DealiAxy, blog Cao thủ vẽ ngôi sao, Phòng thí nghiệm lập trình. Chuyên khám phá các công nghệ mới đang được quan tâm trên internet và thực hành phát triển linh hoạt theo nhóm, bao gồm thiết kế kiến trúc, thuật toán học máy và phân tích dữ liệu, phát triển di động, Linux, phát triển Web frontend/backend, v.v.";
        var keywords = "DealiAxy,Phòng thí nghiệm lập trình,blog,lập trình,công nghệ,phát triển,StarBlog,Cao thủ vẽ ngôi sao,thiết kế chương trình,phát triển Web,học máy,phân tích dữ liệu";

        return new SeoMetadata {
            Title = siteName,
            Description = description,
            Keywords = keywords,
            OgTitle = siteName,
            OgDescription = description,
            OgType = "website",
            OgUrl = GetBaseUrl(),
            TwitterCard = "summary_large_image",
            TwitterTitle = siteName,
            TwitterDescription = description
        };
    }

    public SeoMetadata GetPostSeoMetadata(PostViewModel post) {
        var title = $"{post.Title} - StarBlog";
        var description = !string.IsNullOrEmpty(post.Summary) && post.Summary != "(Không có mô tả)"
            ? post.Summary
            : ExtractDescriptionFromContent(post.Content);

        var keywords = GenerateKeywordsFromPost(post);
        var url = post.Url ?? $"{GetBaseUrl()}/Blog/Post/{post.Id}";

        return new SeoMetadata {
            Title = title,
            Description = description,
            Keywords = keywords,
            OgTitle = post.Title,
            OgDescription = description,
            OgType = "article",
            OgUrl = url,
            OgArticleAuthor = "DealiAxy",
            OgArticlePublishedTime = post.CreationTime,
            OgArticleModifiedTime = post.LastUpdateTime,
            OgArticleSection = post.Category?.Name,
            TwitterCard = "summary_large_image",
            TwitterTitle = post.Title,
            TwitterDescription = description,
            CanonicalUrl = url
        };
    }

    public SeoMetadata GetCategorySeoMetadata(Category category, int postCount) {
        var title = $"{category.Name} - Danh mục - StarBlog";
        var description = $"StarBlog {category.Name} - tất cả bài viết trong danh mục này, tổng cộng {postCount} bài viết.";
        var keywords = $"{category.Name},danh mục,StarBlog,blog,bài viết kỹ thuật";
        var url = $"{GetBaseUrl()}/Blog/List?categoryId={category.Id}";

        return new SeoMetadata {
            Title = title,
            Description = description,
            Keywords = keywords,
            OgTitle = title,
            OgDescription = description,
            OgType = "website",
            OgUrl = url,
            TwitterCard = "summary",
            TwitterTitle = title,
            TwitterDescription = description,
            CanonicalUrl = url
        };
    }

    public SeoMetadata GetBlogListSeoMetadata(int page = 1) {
        var title = page > 1 ? $"Bài viết blog - Trang {page} - StarBlog" : "Bài viết blog - StarBlog";
        var description = "StarBlog Danh sách bài viết blog, chia sẻ kỹ thuật lập trình, kinh nghiệm phát triển, hướng dẫn kỹ thuật và các nội dung khác.";
        var keywords = "blog,danh sách bài viết,lập trình,công nghệ,phát triển,StarBlog";
        var url = page > 1 ? $"{GetBaseUrl()}/Blog/List?page={page}" : $"{GetBaseUrl()}/Blog/List";

        return new SeoMetadata {
            Title = title,
            Description = description,
            Keywords = keywords,
            OgTitle = title,
            OgDescription = description,
            OgType = "website",
            OgUrl = url,
            TwitterCard = "summary",
            TwitterTitle = title,
            TwitterDescription = description,
            CanonicalUrl = url
        };
    }

    private string ExtractDescriptionFromContent(string content) {
        if (string.IsNullOrEmpty(content)) return "StarBlog bài viết blog kỹ thuật";

        // Loại bỏ ký hiệu Markdown
        var plainText = System.Text.RegularExpressions.Regex.Replace(content, @"[#*`\[\]()_~]", "");
        plainText = System.Text.RegularExpressions.Regex.Replace(plainText, @"!\[.*?\]\(.*?\)", "");
        plainText = System.Text.RegularExpressions.Regex.Replace(plainText, @"\[.*?\]\(.*?\)", "");

        // Lấy 160 ký tự đầu làm mô tả
        return plainText.Length > 160 ? plainText.Substring(0, 157) + "..." : plainText;
    }

    private string GenerateKeywordsFromPost(PostViewModel post) {
        var keywords = new List<string> { "DealiAxy", "StarBlog", "blog", "công nghệ" };

        keywords.Add(post.Category.Name);

        // Trích xuất từ khóa từ tiêu đề
        var titleWords = post.Title.Split(new[] { ' ', '-', '_', ':', '：', '，', '。' },
            StringSplitOptions.RemoveEmptyEntries);
        keywords.AddRange(titleWords.Where(w => w.Length > 1).Take(3));

        return string.Join(",", keywords.Distinct());
    }

    private string GetBaseUrl() {
        var host = _configService["host"];
        return !string.IsNullOrEmpty(host) ? host.TrimEnd('/') : "https://blog.deali.cn";
    }
}

public class SeoMetadata {
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public string Keywords { get; set; } = "";
    public string? CanonicalUrl { get; set; }

    // Open Graph
    public string OgTitle { get; set; } = "";
    public string OgDescription { get; set; } = "";
    public string OgType { get; set; } = "website";
    public string OgUrl { get; set; } = "";
    public string? OgImage { get; set; }
    public string? OgArticleAuthor { get; set; }
    public DateTime? OgArticlePublishedTime { get; set; }
    public DateTime? OgArticleModifiedTime { get; set; }
    public string? OgArticleSection { get; set; }

    // Twitter Card
    public string TwitterCard { get; set; } = "summary";
    public string TwitterTitle { get; set; } = "";
    public string TwitterDescription { get; set; } = "";
    public string? TwitterImage { get; set; }
}