using StarBlog.Data.Models;

namespace StarBlog.Web.ViewModels.Search;

public class SearchPost {
    public Post Post { get; set; }
    public int TitleScore { get; set; }

    public int ContentScore { get; set; }

    // Mỗi lần khớp tiêu đề +100 điểm
    // Khớp nội dung +1 điểm
    public int Score => TitleScore * 100 + ContentScore;
    public string HighlightedTitle { get; set; }
    public string HighlightedSnippet { get; set; }
}