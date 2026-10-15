using StarBlog.Application.Criteria;

namespace StarBlog.Application.Services.VisitRecordServices;

public static class VisitRecordQueryExtensions {
    public static IQueryable<Data.Models.VisitRecord> ApplyFilters(
        this IQueryable<Data.Models.VisitRecord> query, VisitRecordParameters p
    ) {
        if (p.ExcludeApi == true) query = query.Where(r => !r.RequestPath.StartsWith("/api/"));
        if (p.ExcludeIntranetIp == true)
            // todo Logic kiểm tra null kiểu này có thể có vấn đề, với IP không phải mạng nội bộ thì thông tin ISP có thể là null
            // Lưu ý: dữ liệu ip2region trả về trường ISP bằng tiếng Trung, IP mạng nội bộ có giá trị chứa "内网" nên phải giữ nguyên chuỗi này để so khớp.
            query = query.Where(r => r.IpInfo.Isp != null && !r.IpInfo.Isp.Contains("内网"));


        if (p.Date.HasValue) {
            var startDate = p.Date.Value.Date;
            var endDate = startDate.AddDays(1);
            query = query.Where(r => r.Time >= startDate && r.Time < endDate);
        }

        if (p.From.HasValue) query = query.Where(r => r.Time >= p.From.Value);
        if (p.To.HasValue) query = query.Where(r => r.Time <= p.To.Value);

        // Lọc liên quan đến thông tin IP
        if (!string.IsNullOrEmpty(p.Country)) query = query.Where(r => r.IpInfo.Country == p.Country);
        if (!string.IsNullOrEmpty(p.Province)) query = query.Where(r => r.IpInfo.Province == p.Province);
        if (!string.IsNullOrEmpty(p.City)) query = query.Where(r => r.IpInfo.City == p.City);
        if (!string.IsNullOrEmpty(p.Isp)) query = query.Where(r => r.IpInfo.Isp == p.Isp);

        // Lọc liên quan đến user agent
        if (!string.IsNullOrEmpty(p.OS)) query = query.Where(r => r.UserAgentInfo.OS.Family == p.OS);
        if (!string.IsNullOrEmpty(p.Device)) query = query.Where(r => r.UserAgentInfo.Device.Family == p.Device);
        if (!string.IsNullOrEmpty(p.UserAgent))
            query = query.Where(r => r.UserAgent != null && r.UserAgent.Contains(p.UserAgent));
        if (p.IsSpider.HasValue) query = query.Where(r => r.UserAgentInfo.Device.IsSpider == p.IsSpider.Value);

        // Xử lý tham số truy vấn chung
        // Tìm kiếm theo từ khóa
        if (!string.IsNullOrEmpty(p.Search)) {
            query = query.Where(r =>
                r.Referrer != null && r.UserAgent != null && r.Ip != null && (
                    r.RequestPath.Contains(p.Search) ||
                    r.Referrer.Contains(p.Search) ||
                    r.UserAgent.Contains(p.Search) ||
                    r.Ip.Contains(p.Search))
            );
        }

        // Xử lý sắp xếp
        if (!string.IsNullOrEmpty(p.SortBy)) {
            // Xử lý hướng sắp xếp, mặc định tăng dần, nếu bắt đầu bằng '-' thì giảm dần
            bool isDescending = p.SortBy.StartsWith("-");
            string sortField = isDescending ? p.SortBy[1..] : p.SortBy;

            // Sắp xếp theo trường sắp xếp
            switch (sortField.ToLower()) {
                case "time":
                    query = isDescending ? query.OrderByDescending(r => r.Time) : query.OrderBy(r => r.Time);
                    break;
                case "ip":
                    query = isDescending ? query.OrderByDescending(r => r.Ip) : query.OrderBy(r => r.Ip);
                    break;
                case "requestpath":
                    query = isDescending
                        ? query.OrderByDescending(r => r.RequestPath)
                        : query.OrderBy(r => r.RequestPath);
                    break;
                case "referer":
                    query = isDescending ? query.OrderByDescending(r => r.Referrer) : query.OrderBy(r => r.Referrer);
                    break;
                case "useragent":
                    query = isDescending ? query.OrderByDescending(r => r.UserAgent) : query.OrderBy(r => r.UserAgent);
                    break;
                default:
                    // Mặc định sắp xếp theo thời gian giảm dần
                    query = query.OrderByDescending(r => r.Time);
                    break;
            }
        }
        else {
            // Mặc định sắp xếp theo thời gian giảm dần
            query = query.OrderByDescending(r => r.Time);
        }

        return query;
    }
}