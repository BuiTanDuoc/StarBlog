using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace StarBlog.Web.Criteria;

public class VisitRecordParameters : QueryParameters {
    /// <summary>
    /// Trường sắp xếp
    /// </summary>
    public new string? SortBy { get; set; } = "-Time";

    /// <summary>
    /// Loại trừ API
    /// </summary>
    public bool? ExcludeApi { get; set; } = false;

    /// <summary>
    /// Loại trừ IP mạng nội bộ
    /// </summary>
    public bool? ExcludeIntranetIp { get; set; }

    public DateTime? Date { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }

    /// <summary>
    /// Quốc gia
    /// </summary>
    public string? Country { get; set; }

    /// <summary>
    /// Tỉnh/bang
    /// </summary>
    public string? Province { get; set; }

    /// <summary>
    /// Thành phố
    /// </summary>
    public string? City { get; set; }

    /// <summary>
    /// Nhà mạng (ISP)
    /// </summary>
    public string? Isp { get; set; }

    /// <summary>
    /// Hệ điều hành
    /// </summary>
    [JsonPropertyName("os")]
    [JsonProperty("os")]
    public string? OS { get; set; }

    /// <summary>
    /// Thiết bị
    /// </summary>
    public string? Device { get; set; }

    /// <summary>
    /// UA
    /// </summary>
    public string? UserAgent { get; set; }

    /// <summary>
    /// Có phải crawler hay không
    /// </summary>
    public bool? IsSpider { get; set; }
}