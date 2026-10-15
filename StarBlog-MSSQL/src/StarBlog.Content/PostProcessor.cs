using System.Text.RegularExpressions;
using System.Web;
using Markdig;
using Markdig.Renderers.Normalize;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using StarBlog.Content.Extensions;
using StarBlog.Content.Utils;
using StarBlog.Data.Models;

namespace StarBlog.Content;

public class PostProcessor {
    private readonly Post _post;
    private readonly string _importPath;
    private readonly string _assetsPath;

    public PostProcessor(string importPath, string assetsPath, Post post) {
        _post = post;
        _assetsPath = assetsPath;
        _importPath = importPath;
    }

    /// <summary>
    /// Phân tích nội dung Markdown, sao chép hình ảnh & thay thế liên kết hình ảnh
    /// </summary>
    public string MarkdownParse() {
        if (_post.Content == null) {
            return string.Empty;
        }

        var document = Markdown.Parse(_post.Content);

        foreach (var node in document.AsEnumerable()) {
            if (node is not ParagraphBlock { Inline: { } } paragraphBlock) continue;
            foreach (var inline in paragraphBlock.Inline) {
                if (inline is not LinkInline { IsImage: true } linkInline) continue;
                if (string.IsNullOrWhiteSpace(linkInline.Url)) continue;

                var imgUrl = Uri.UnescapeDataString(linkInline.Url);
                if (imgUrl.StartsWith("http")) continue;

                var imgPath = Path.Combine(_importPath, _post.Path ?? "", imgUrl);
                var imgFilename = Path.GetFileName(imgUrl);
                var destDir = Path.Combine(_assetsPath, _post.Id);
                if (!Directory.Exists(destDir)) Directory.CreateDirectory(destDir);

                var destPath = Path.Combine(destDir, imgFilename);
                if (File.Exists(destPath)) {
                    var imgId = GuidUtils.GuidTo16String();
                    imgFilename =
                        $"{Path.GetFileNameWithoutExtension(imgFilename)}-{imgId}.{Path.GetExtension(imgFilename)}";
                    destPath = Path.Combine(destDir, imgFilename);
                }

                linkInline.Url = imgFilename;
                File.Copy(imgPath, destPath);

                Console.WriteLine($"Sao chép {imgPath} đến {destPath}");
            }
        }

        using var writer = new StringWriter();
        var render = new NormalizeRenderer(writer);
        render.Render(document);
        return writer.ToString();
    }

    /// <summary>
    /// Trích xuất phần tóm tắt từ <paramref name="length"/> ký tự đầu của nội dung bài viết
    /// </summary>
    public string GetSummary(int length) {
        return _post.Content == null
            ? string.Empty
            : Markdown.ToPlainText(_post.Content).Limit(length);
    }

    /// <summary>
    /// Điền trạng thái và tiêu đề bài viết
    /// </summary>
    public (string, string) InflateStatusTitle() {
        // Quy ước tiêu đề: "(Trạng thái)Tiêu đề". Vẫn chấp nhận cả dấu ngoặc toàn chiều rộng (full-width) của dữ liệu cũ.
        const string pattern = @"^[(（](.+)[)）](.+)$";
        var status = _post.Status ?? "Đã xuất bản";
        var title = _post.Title;
        if (string.IsNullOrEmpty(title)) return (status, $"Bài viết chưa đặt tên {_post.CreationTime.ToLongDateString()}");

        var result = Regex.Match(title, pattern);
        if (!result.Success) return (status, title);

        status = result.Groups[1].Value;
        title = result.Groups[2].Value;

        _post.Status = status;
        _post.Title = title;

        if (!new[] { "Đã đăng", "Đã xuất bản" }.Contains(_post.Status)) {
            _post.IsPublish = false;
        }

        return (status, title);
    }
}
