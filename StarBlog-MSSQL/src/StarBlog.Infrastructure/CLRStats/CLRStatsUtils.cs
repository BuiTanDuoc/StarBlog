namespace StarBlog.Infrastructure.CLRStats;

/// <summary>
/// Plugin thống kê tình hình sử dụng tài nguyên của ứng dụng .NET, gồm: mức sử dụng CPU, GC, tình trạng thread, hỗ trợ lấy thông tin trạng thái qua Web request (có thể tùy chỉnh đường dẫn truy cập và xác thực danh tính), dữ liệu sẽ được trả về ở định dạng JSON.
/// <para></para>
/// <para>Địa chỉ dự án gốc: https://github.com/itsvse/CLRStats</para>
/// </summary>
public static class CLRStatsUtils {
    private static readonly CLRStatsModel CLrStatsModel = new CLRStatsModel();

    public static CLRStatsModel GetCurrentClrStats() {
        return CLrStatsModel;
    }

    public static string GetCurrentClrStatsToJson() {
        return GetCurrentClrStats().ToJson();
    }
}
