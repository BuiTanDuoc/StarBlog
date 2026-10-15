using System.Text.RegularExpressions;
using FreeSql;
using Microsoft.AspNetCore.Mvc;
using StarBlog.Data.Models;
using StarBlog.Web.ViewModels.Search;

namespace StarBlog.Web.Controllers;

public class SearchController : Controller {
    private readonly IBaseRepository<Post> _postRepo;
    private readonly IBaseRepository<Category> _categoryRepo;

    public SearchController(IBaseRepository<Post> postRepo, IBaseRepository<Category> categoryRepo) {
        _postRepo = postRepo;
        _categoryRepo = categoryRepo;
    }

    public IActionResult Blog(string keyword, int categoryId = 0, int page = 1, int pageSize = 5) {
        var searchPosts = _postRepo
            .Where(a => a.IsPublish)
            .Where(a =>
                a.Title!.Contains(keyword) ||
                a.Content.Contains(keyword)
            )
            .Include(a => a.Category)
            .ToList()
            .Select(p => new SearchPost {
                Post = p,
                TitleScore = p.Title.ToLower().Split(keyword).Length - 1,
                ContentScore = p.Content?.ToLower().Split(keyword).Length - 1 ?? 0,
            })
            .OrderByDescending(x => x.Score)
            .ToList();

        // Hiện thực hóa việc làm nổi bật kết quả tìm kiếm
        var regex = new Regex(Regex.Escape(keyword), RegexOptions.IgnoreCase);
        foreach (var item in searchPosts) {
            item.HighlightedTitle = regex.Replace(item.Post.Title, m => $"<mark>{m.Value}</mark>");
            item.HighlightedSnippet = GetHighlightedSnippet(item.Post.Content, keyword);
        }

        return View("Result", new SearchResultViewModel {
            Keyword = keyword,
            SearchPosts = searchPosts
        });
    }

    public static string GetHighlightedSnippet(string content, string keyword, int snippetLength = 100) {
        if (string.IsNullOrEmpty(content) || string.IsNullOrEmpty(keyword))
            return string.Empty;

        var regex = new Regex(Regex.Escape(keyword), RegexOptions.IgnoreCase);
        var match = regex.Match(content);

        if (!match.Success) {
            // Không khớp, lấy trực tiếp snippetLength*2 ký tự đầu làm đoạn tóm tắt
            return content.Length > snippetLength * 2
                ? content.Substring(0, snippetLength * 2) + "..."
                : content;
        }

        // Tính phạm vi cắt (mỗi bên của vị trí khớp lấy snippetLength)
        int start = Math.Max(0, match.Index - snippetLength);
        int length = Math.Min(content.Length - start, match.Length + snippetLength * 2);

        string snippet = content.Substring(start, length);

        // Xử lý làm nổi bật
        snippet = regex.Replace(snippet, m => $"<mark>{m.Value}</mark>");

        // Thêm dấu ba chấm ở hai đầu (nếu không phải đầu hoặc cuối toàn văn)
        if (start > 0) snippet = "..." + snippet;
        if (start + length < content.Length) snippet += "...";

        return snippet;
    }
}