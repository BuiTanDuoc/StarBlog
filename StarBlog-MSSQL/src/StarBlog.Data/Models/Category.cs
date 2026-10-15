using FreeSql.DataAnnotations;

namespace StarBlog.Data.Models;

public class Category {
    [Column(IsIdentity = true, IsPrimary = true)]
    public int Id { get; set; }

    public string Name { get; set; }

    public int ParentId { get; set; }
    [Navigate(nameof(ParentId))]
    public Category? Parent { get; set; }
    [Navigate(nameof(ParentId))]
    public List<Category> Children { get; set; }

    /// <summary>
    /// Danh mục có hiển thị hay không
    /// </summary>
    public bool Visible { get; set; } = true;

    public List<Post> Posts { get; set; }
}