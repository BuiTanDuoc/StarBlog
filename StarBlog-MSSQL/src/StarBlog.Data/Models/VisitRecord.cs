using FreeSql.DataAnnotations;

namespace StarBlog.Data.Models;

/// <summary>
/// Bản ghi truy cập
/// </summary>
[Table(Name = "visit_record")]
public class VisitRecord {
    [Column(IsIdentity = true, IsPrimary = true)]
    public long Id { get; set; }

    /// <summary>
    /// Địa chỉ IP
    /// </summary>
    public string? Ip { get; set; }

    /// <summary>
    /// Thông tin IP
    /// </summary>
    public IpInfo IpInfo { get; set; }

    /// <summary>
    /// Đường dẫn yêu cầu
    /// </summary>
    public string RequestPath { get; set; }

    /// <summary>
    /// Tham số yêu cầu
    /// </summary>
    public string? RequestQueryString { get; set; }

    /// <summary>
    /// Phương thức yêu cầu
    /// </summary>
    public string RequestMethod { get; set; }

    /// <summary>
    /// Thiết bị của người dùng
    /// </summary>
    public string? UserAgent { get; set; }

    /// <summary>
    /// Thông tin UA
    /// </summary>
    public UserAgentInfo UserAgentInfo { get; set; } = new();

    /// <summary>
    /// Thời gian
    /// </summary>
    public DateTime Time { get; set; }

    /// <summary>HTTP Mã trạng thái</summary>
    public int StatusCode { get; set; }

    /// <summary>Thời gian phản hồi (ms)</summary>
    public int ResponseTimeMs { get; set; }

    /// <summary>Trang nguồn</summary>
    public string? Referrer { get; set; }
}

public class IpInfo {
    // Khu vực
    public string? RegionCode { get; set; }
    public string? Country { get; set; }
    public string? Province { get; set; }
    public string? City { get; set; }
    public string? Isp { get; set; }
}

public class UserAgentInfo {
    public OS OS { get; set; } = new();
    public Device Device { get; set; } = new();
    public UserAgent UserAgent { get; set; } = new();
}

public class OS {
    public string? Family { get; set; }
    public string? Major { get; set; }
    public string? Minor { get; set; }
    public string? Patch { get; set; }
    public string? PatchMinor { get; set; }
}

public class Device {
    public string? Brand { get; set; }
    public string? Family { get; set; }
    public string? Model { get; set; }
    public bool IsSpider { get; set; }
}

public class UserAgent {
    public string? Family { get; set; }
    public string? Major { get; set; }
    public string? Minor { get; set; }
    public string? Patch { get; set; }
}