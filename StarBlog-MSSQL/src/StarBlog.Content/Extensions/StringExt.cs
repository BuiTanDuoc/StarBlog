using StarBlog.Content.Utils;

namespace StarBlog.Content.Extensions;

public static class StringExt {
    public static string Limit(this string str, int length) {
        return str.Length <= length ? str : str[..length];
    }

    /// <summary>
    /// Giới hạn độ dài hiển thị của chuỗi và thêm dấu ba chấm ở cuối
    /// </summary>
    public static string LimitWithEllipsis(this string str, int length) {
        return str.Length <= length ? str : $"{str[..length]}...";
    }

    public static string ToSHA256(this string source) {
        return HashUtils.ComputeSHA256Hash(source);
    }

    public static string ToSHA384(this string source) {
        return HashUtils.ComputeSHA384Hash(source);
    }

    public static string ToSHA512(this string source) {
        return HashUtils.ComputeSHA512Hash(source);
    }
}
