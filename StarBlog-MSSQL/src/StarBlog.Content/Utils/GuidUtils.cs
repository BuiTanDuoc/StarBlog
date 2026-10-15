namespace StarBlog.Content.Utils;

public static class GuidUtils {
    /// <summary>
    /// Số 32 chữ số được phân cách bằng dấu gạch nối
    /// </summary>
    private static string GetGuid() {
        var guid = new Guid();
        guid = Guid.NewGuid();
        return guid.ToString();
    }

    /// <summary>  
    /// Lấy chuỗi duy nhất 16 ký tự từ GUID  
    /// </summary>  
    public static string GuidTo16String() {
        long i = 1;
        foreach (var b in Guid.NewGuid().ToByteArray()) {
            i *= b + 1;
        }

        return $"{i - DateTime.Now.Ticks:x}";
    }

    /// <summary>  
    /// Lấy dãy số duy nhất 19 chữ số từ GUID  
    /// </summary>  
    public static long GuidToLongID() {
        var buffer = Guid.NewGuid().ToByteArray();
        return BitConverter.ToInt64(buffer, 0);
    }
}
