namespace StarBlog.Application.Criteria;

/// <summary>
/// Tham số yêu cầu
/// </summary>
public class QueryParameters {
    /// <summary>
    /// Số mục tối đa mỗi trang
    /// </summary>
    public const int MaxPageSize = 50;

    private int _pageSize = 10;

    /// <summary>
    /// Có bật phân trang hay không (mặc định true)
    /// </summary>
    public bool EnablePaging { get; set; } = true;

    /// <summary>
    /// Kích thước trang
    /// </summary>
    public int PageSize {
        get => _pageSize;
        set => _pageSize = (value > MaxPageSize) ? MaxPageSize : value;
    }

    /// <summary>
    /// Số trang hiện tại
    /// </summary>
    public int Page { get; set; } = 1;

    /// <summary>
    /// Từ khóa tìm kiếm
    /// </summary>
    public string? Search { get; set; }

    /// <summary>
    /// Trường sắp xếp
    /// </summary>
    public string? SortBy { get; set; }
}