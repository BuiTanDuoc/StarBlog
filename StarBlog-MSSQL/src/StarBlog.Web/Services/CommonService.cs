using System.Net;
using StarBlog.Content.Utils;

namespace StarBlog.Web.Services;

/// <summary>
/// Một số dịch vụ dùng chung
/// </summary>
public class CommonService {
    private readonly ILogger<CommonService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    public CommonService(ILogger<CommonService> logger, IHttpClientFactory httpClientFactory) {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }

    /// <summary>
    /// Tải xuống tệp
    /// </summary>
    /// <param name="url"></param>
    /// <param name="savePath">Đường dẫn lưu, cần đường dẫn đầy đủ</param>
    /// <returns></returns>
    public async Task<string?> DownloadFileAsync(string url, string savePath) {
        var httpClient = _httpClientFactory.CreateClient();
        try {
            var resp = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);

            var fileName = GuidUtils.GuidTo16String() + Path.GetExtension(url);
            var filePath = Path.Combine(savePath, WebUtility.UrlEncode(fileName));
            await using var fs = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write);
            await resp.Content.CopyToAsync(fs);

            return fileName;
        }
        catch (Exception ex) {
            _logger.LogError("Lỗi khi tải xuống tệp, thông tin: {Error}", ex);
            return null;
        }
    }
}
