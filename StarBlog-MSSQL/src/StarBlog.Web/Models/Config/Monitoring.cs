namespace StarBlog.Web.Models.Config;

/// <summary>
/// Cấu hình giám sát
/// </summary>
public class Monitoring
{
    /// <summary>
    /// Umami Cấu hình phân tích website
    /// </summary>
    public UmamiConfig Umami { get; set; } = new();
    
    /// <summary>
    /// Microsoft Clarity Cấu hình
    /// </summary>
    public ClarityConfig Clarity { get; set; } = new();
}

/// <summary>
/// Umami Cấu hình
/// </summary>
public class UmamiConfig
{
    /// <summary>
    /// Có bật phân tích Umami hay không
    /// </summary>
    public bool Enabled { get; set; }
    
    /// <summary>
    /// Umami Địa chỉ script
    /// </summary>
    public string ScriptUrl { get; set; } = string.Empty;
    
    /// <summary>
    /// Website ID
    /// </summary>
    public string WebsiteId { get; set; } = string.Empty;
}

/// <summary>
/// Microsoft Clarity Cấu hình
/// </summary>
public class ClarityConfig
{
    /// <summary>
    /// Có bật phân tích Clarity hay không
    /// </summary>
    public bool Enabled { get; set; }
    
    /// <summary>
    /// Clarity Dự án ID
    /// </summary>
    public string ProjectId { get; set; } = string.Empty;
}