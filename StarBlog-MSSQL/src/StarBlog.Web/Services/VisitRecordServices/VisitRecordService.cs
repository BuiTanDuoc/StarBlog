using System.Linq.Dynamic.Core;
using Microsoft.EntityFrameworkCore;
using StarBlog.Data;
using StarBlog.Data.Models;
using StarBlog.Web.Criteria;
using StarBlog.Web.Extensions;
using StarBlog.Web.ViewModels.VisitRecord;
using X.PagedList;

namespace StarBlog.Web.Services.VisitRecordServices;

public class VisitRecordAnalyticsService {
    private readonly ILogger<VisitRecordAnalyticsService> _logger;
    private readonly AppDbContext _dbContext;

    public VisitRecordAnalyticsService(ILogger<VisitRecordAnalyticsService> logger, AppDbContext dbContext) {
        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task<VisitRecord?> GetById(int id) {
        var item = await _dbContext.VisitRecords.FirstOrDefaultAsync(e => e.Id == id);
        return item;
    }

    public async Task<List<VisitRecord>> GetAll(VisitRecordParameters p) {
        return await _dbContext.VisitRecords.ApplyFilters(p).ToListAsync();
    }

    public Task<IPagedList<VisitRecord>> GetPagedList(VisitRecordParameters p) {
        var qs = _dbContext.VisitRecords.ApplyFilters(p);
        return qs.ToPagedListAsync(p);
    }

    /// <summary>
    /// Dữ liệu tổng quan
    /// </summary>
    /// <returns></returns>
    public async Task<VisitOverview> Overview(VisitRecordParameters p) {
        var qs = _dbContext.VisitRecords.ApplyFilters(p);

        var data = new VisitOverview {
            Total = await qs.CountAsync(),
            Pv = qs.Count(e =>
                !e.RequestPath.ToLower().StartsWith("/api") &&
                !e.UserAgentInfo.Device.IsSpider
            ),
            Uv = qs.Where(e =>
                    !e.RequestPath.ToLower().StartsWith("/api") &&
                    !e.UserAgentInfo.Device.IsSpider)
                .Select(e => e.Ip).Distinct().Count(),
            Api = qs.Count(e => e.RequestPath.ToLower().StartsWith("/api")),
            Spider = qs.Count(e => e.UserAgentInfo.Device.IsSpider),
        };

        return data;
    }

    /// <summary>
    /// Lấy dữ liệu xu hướng theo ngày - dùng truy vấn tách nhỏ để tránh vấn đề dịch LINQ phức tạp
    /// </summary>
    /// <param name="p"></param>
    /// <param name="days">Xem dữ liệu của vài ngày gần đây, mặc định 7 ngày</param>
    /// <returns></returns>
    public async Task<List<DailyTrend>> GetDailyTrend(VisitRecordParameters p, int days) {
        var qs = _dbContext.VisitRecords.ApplyFilters(p);
        var startDate = DateTime.Today.AddDays(-days).Date;

        // Lấy dữ liệu cơ bản vào bộ nhớ để xử lý
        var records = await qs.Where(e => e.Time.Date >= startDate)
            .Select(e => new {
                e.Time.Date,
                e.RequestPath,
                e.Ip,
                IsSpider = e.UserAgentInfo.Device.IsSpider
            })
            .ToListAsync();

        // Nhóm và thống kê trong bộ nhớ
        var data = records
            .GroupBy(e => e.Date)
            .Select(g => new DailyTrend {
                Time = g.Key,
                Date = $"{g.Key.Month}-{g.Key.Day}",
                // Tổng lượt truy cập
                Total = g.Count(),
                // page view (exclude api and spiders)
                Pv = g.Count(e =>
                    !e.RequestPath.ToLower().StartsWith("/api") &&
                    !e.IsSpider
                ),
                // unique visitors
                Uv = g.Where(e =>
                        !e.RequestPath.ToLower().StartsWith("/api") &&
                        !e.IsSpider)
                    .Select(e => e.Ip).Distinct().Count(),
                // api visit count
                Api = g.Count(e => e.RequestPath.ToLower().StartsWith("/api")),
                Spider = g.Count(e => e.IsSpider),
            })
            .OrderBy(e => e.Time)
            .ToList();

        return data;
    }

    /// <summary>
    /// Xu hướng theo giờ - dùng truy vấn tách nhỏ để tránh vấn đề dịch LINQ phức tạp
    /// <para>Xem biến động lượt truy cập theo giờ</para>
    /// </summary>
    public async Task<List<HourlyTrend>> GetHourlyTrend(VisitRecordParameters p) {
        var qs = _dbContext.VisitRecords.ApplyFilters(p);
        
        // Lấy dữ liệu cơ bản vào bộ nhớ để xử lý
        var records = await qs
            .Select(e => new {
                e.Time.Hour,
                e.RequestPath,
                e.Ip,
                IsSpider = e.UserAgentInfo.Device.IsSpider
            })
            .ToListAsync();

        // Nhóm và thống kê trong bộ nhớ
        var data = records
            .GroupBy(e => e.Hour)
            .Select(g => new HourlyTrend {
                Hour = g.Key,
                // Tổng lượt truy cập
                Total = g.Count(),
                // page view (exclude api and spiders)
                Pv = g.Count(e =>
                    !e.RequestPath.ToLower().StartsWith("/api") &&
                    !e.IsSpider
                ),
                // unique visitors
                Uv = g.Where(e =>
                        !e.RequestPath.ToLower().StartsWith("/api") &&
                        !e.IsSpider)
                    .Select(e => e.Ip).Distinct().Count(),
                // api visit count
                Api = g.Count(e => e.RequestPath.ToLower().StartsWith("/api")),
                Spider = g.Count(e => e.IsSpider),
            })
            .OrderBy(e => e.Hour)
            .ToList();

        return data;
    }

    /// <summary>
    /// Lấy tham số lọc thông tin địa lý
    /// </summary>
    public async Task<List<string?>> GetGeoFilterParams(VisitRecordParameters p, string param = "country") {
        var qs = _dbContext.VisitRecords.ApplyFilters(p);
        return param switch {
            "country" => await qs.Select(e => e.IpInfo.Country).Distinct().ToListAsync(),
            "province" => await qs.Select(e => e.IpInfo.Province).Distinct().ToListAsync(),
            "city" => await qs.Select(e => e.IpInfo.City).Distinct().ToListAsync(),
            _ => new List<string?>()
        };
    }

    public async Task<UserAgentFilterParams> GetUserAgentFilterParams(VisitRecordParameters p) {
        var qs = _dbContext.VisitRecords.ApplyFilters(p);
        return new UserAgentFilterParams {
            OS = await qs.Select(e => e.UserAgentInfo.OS.Family).Distinct().ToListAsync(),
            Device = await qs.Select(e => e.UserAgentInfo.Device.Family).Distinct().ToListAsync(),
            UserAgent = await qs.Select(e => e.UserAgentInfo.UserAgent.Family).Distinct().ToListAsync(),
        };
    }

    /// <summary>
    /// PV / UV Thống kê
    /// <para>PV (Page Views): Tổng số lượt truy cập</para>
    /// <para>UV (Unique Visitors): Số IP đã loại trùng hoặc số người dùng đã loại trùng</para>
    /// </summary>
    public async Task<PvUv> GetPvUv(VisitRecordParameters p) {
        var qs = _dbContext.VisitRecords.ApplyFilters(p);
        var pv = await qs.CountAsync();
        var uv = await qs.Select(e => e.Ip).Distinct().CountAsync();
        return new PvUv { PV = pv, UV = uv };
    }

    /// <summary>
    /// Top N Đường dẫn truy cập
    /// <para>Tìm các trang/API phổ biến nhất trong một ngày hoặc một khoảng thời gian</para>
    /// </summary>
    public async Task<List<TopPath>> GetTopPaths(VisitRecordParameters p, int top = 10) {
        var qs = _dbContext.VisitRecords.ApplyFilters(p);
        return await qs.GroupBy(e => e.RequestPath)
            .Select(g => new TopPath { Path = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Take(top)
            .ToListAsync();
    }

    /// <summary>
    /// Phân bố thời gian phản hồi
    /// <para>Tính trung bình, lớn nhất, phân vị thứ 95, v.v.</para>
    /// </summary>
    public async Task<object> GetResponseTimeStats(VisitRecordParameters p) {
        var q = _dbContext.VisitRecords.ApplyFilters(p);
        var avg = await q.AverageAsync(e => e.ResponseTimeMs);
        var max = await q.MaxAsync(e => e.ResponseTimeMs);
        var p95 = await q
            .OrderBy(e => e.ResponseTimeMs)
            .Skip((int)(await q.CountAsync() * 0.95))
            .Select(e => e.ResponseTimeMs)
            .FirstOrDefaultAsync();
        return new { Avg = avg, Max = max, P95 = p95 };
    }

    /// <summary>
    /// Phân bố mã trạng thái
    /// <para>Thống kê số lượt gọi của từng mã trạng thái</para>
    /// </summary>
    public async Task<List<StatusCodeDistribution>> GetStatusCodeDistribution(VisitRecordParameters p) {
        var qs = _dbContext.VisitRecords.ApplyFilters(p);
        return await qs
            .GroupBy(e => e.StatusCode)
            .Select(g => new StatusCodeDistribution { Code = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .ToListAsync();
    }

    /// <summary>
    /// Thống kê phân bố vị trí địa lý
    /// </summary>
    public async Task<object> GetGeoDistribution(VisitRecordParameters p) {
        var qs = _dbContext.VisitRecords.ApplyFilters(p);

        var countryStats = await qs.GroupBy(e => e.IpInfo.Country)
            .Select(g => new { Country = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .ToListAsync();
        var provinceStats = await qs.GroupBy(e => e.IpInfo.Province)
            .Select(g => new { Province = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .ToListAsync();
        var cityStats = await qs.GroupBy(e => e.IpInfo.City)
            .Select(g => new { City = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .ToListAsync();
        var ispStats = await qs.GroupBy(e => e.IpInfo.Isp)
            .Select(g => new { Isp = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .ToListAsync();

        return new {
            Country = countryStats,
            Province = provinceStats,
            City = cityStats,
            Isp = ispStats
        };
    }

    /// <summary>
    /// Phân tích tên miền nguồn
    /// <para>Thống kê phân bố tên miền của nguồn truy cập</para>
    /// </summary>
    public async Task<List<ReferrerDomain>> GetReferrerDomains(VisitRecordParameters p, int top = 10) {
        var qs = _dbContext.VisitRecords.ApplyFilters(p);
        // Trước tiên lấy các bản ghi referrer hợp lệ
        var records = await qs.Where(e => !string.IsNullOrEmpty(e.Referrer))
            .Select(e => e.Referrer)
            .ToListAsync();

        // Phân tích Uri và nhóm trong bộ nhớ
        return records
            .Select(r => {
                try {
                    return new Uri(r!).Host;
                }
                catch {
                    return r;
                } // Xử lý trường hợp không hợp lệ của URL
            })
            .GroupBy(host => host)
            .Select(g => new ReferrerDomain { Domain = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Take(top)
            .ToList();
    }

    /// <summary>
    /// Phân tích tăng trưởng so với kỳ trước
    /// <para>So sánh lượt truy cập giữa chu kỳ hiện tại và chu kỳ trước</para>
    /// </summary>
    public async Task<GrowthRate> GetGrowthRate(VisitRecordParameters p, int days = 7) {
        var qs = _dbContext.VisitRecords.ApplyFilters(p);
        var endDate = DateTime.Today;
        var startDate = endDate.AddDays(-days);
        var prevStartDate = startDate.AddDays(-days);

        var currentPeriodCount = await qs
            .Where(e => e.Time.Date >= startDate && e.Time.Date <= endDate)
            .CountAsync();

        var prevPeriodCount = await qs
            .Where(e => e.Time.Date >= prevStartDate && e.Time.Date < startDate)
            .CountAsync();

        var growthRate = prevPeriodCount == 0 ? 0 : (currentPeriodCount - prevPeriodCount) * 100.0 / prevPeriodCount;

        return new GrowthRate {
            CurrentPeriodCount = currentPeriodCount,
            PrevPeriodCount = prevPeriodCount,
            GrowthPercentage = growthRate,
            Days = days
        };
    }

    /// <summary>
    /// Phân tích tỷ lệ thoát
    /// <para>Tính tỷ lệ phiên chỉ truy cập một trang rồi rời đi</para>
    /// </summary>
    public async Task<BounceRateStats> GetBounceRate(VisitRecordParameters p) {
        var qs = _dbContext.VisitRecords.ApplyFilters(p);

        // Nhóm theo IP và thời gian phiên, tính số trang được truy cập trong mỗi phiên
        var sessions = await qs
            .GroupBy(e => new { e.Ip, SessionTime = e.Time.Date.AddHours(e.Time.Hour) })
            .Select(g => new { g.Key.Ip, g.Key.SessionTime, PageViews = g.Count() })
            .ToListAsync();

        var totalSessions = sessions.Count;
        var bounceSessions = sessions.Count(s => s.PageViews == 1);
        var bounceRate = totalSessions == 0 ? 0 : (double)bounceSessions / totalSessions * 100;

        return new BounceRateStats {
            TotalSessions = totalSessions,
            BounceSessions = bounceSessions,
            BounceRate = bounceRate
        };
    }

    /// <summary>
    /// Phân tích lần truy cập đầu tiên
    /// <para>Thống kê tỷ lệ khách mới và khách quay lại</para>
    /// </summary>
    public async Task<FirstVisitStats> GetFirstVisitStats(VisitRecordParameters p, int days) {
        var qs = _dbContext.VisitRecords.ApplyFilters(p);

        // Lấy thời gian truy cập lần đầu của tất cả IP
        var firstVisits = await qs
            .GroupBy(e => e.Ip)
            .Select(g => new { Ip = g.Key, FirstVisit = g.Min(e => e.Time) })
            .ToListAsync();

        // Bản ghi truy cập trong khoảng thời gian hiện tại
        var periodStart = DateTime.Today.AddDays(-days);
        var currentPeriodVisits = await qs
            .Where(e => e.Time >= periodStart)
            .Select(e => new { e.Ip, e.Time })
            .ToListAsync();

        // Tính số lượng khách mới
        var newVisitors = currentPeriodVisits
            .GroupBy(e => e.Ip)
            .Count(g => firstVisits.First(f => f.Ip == g.Key).FirstVisit >= periodStart);

        // Tính số lượng khách quay lại
        var returningVisitors = currentPeriodVisits
            .Select(e => e.Ip)
            .Distinct()
            .Count() - newVisitors;

        return new FirstVisitStats {
            NewVisitors = newVisitors,
            ReturningVisitors = returningVisitors,
            TotalVisitors = newVisitors + returningVisitors,
            NewVisitorsPercentage = (double)newVisitors / (newVisitors + returningVisitors) * 100
        };
    }

    /// <summary>
    /// Thống kê phân bố công nghệ
    /// <para>Thống kê phân bố của trình duyệt, thiết bị, hệ điều hành</para>
    /// </summary>
    public async Task<TechDistribution> GetTechDistribution(VisitRecordParameters p) {
        var qs = _dbContext.VisitRecords.ApplyFilters(p);

        var browserStats = await qs
            .GroupBy(e => e.UserAgentInfo.UserAgent.Family)
            .Select(g => new { Browser = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Take(10)
            .ToListAsync();

        var osStats = await qs
            .GroupBy(e => e.UserAgentInfo.OS.Family)
            .Select(g => new { OS = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Take(10)
            .ToListAsync();

        var deviceStats = await qs
            .GroupBy(e => e.UserAgentInfo.Device.Family)
            .Select(g => new { Device = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Take(10)
            .ToListAsync();

        return new TechDistribution {
            Browsers = browserStats.Select(b => new NameCountPair { Name = b.Browser, Count = b.Count }).ToList(),
            OperatingSystems = osStats.Select(o => new NameCountPair { Name = o.OS, Count = o.Count }).ToList(),
            Devices = deviceStats.Select(d => new NameCountPair { Name = d.Device, Count = d.Count }).ToList()
        };
    }

    /// <summary>
    /// Xếp hạng request chậm
    /// <para>Lấy danh sách request có thời gian phản hồi lâu nhất</para>
    /// </summary>
    public async Task<List<SlowRequest>> GetSlowRequests(VisitRecordParameters p, int top = 10) {
        var qs = _dbContext.VisitRecords.ApplyFilters(p);

        return await qs
            .OrderByDescending(e => e.ResponseTimeMs)
            .Take(top)
            .Select(e => new SlowRequest {
                Path = e.RequestPath,
                Method = e.RequestMethod,
                ResponseTimeMs = e.ResponseTimeMs,
                Time = e.Time,
                StatusCode = e.StatusCode
            })
            .ToListAsync();
    }

    /// <summary>
    /// Phân tích hành vi chuyển đổi
    /// <para>Phân tích đường dẫn chuyển đổi của người dùng từ trang vào đến trang đích</para>
    /// </summary>
    public async Task<ConversionStats> GetConversionStats(VisitRecordParameters p, string targetPath) {
        var qs = _dbContext.VisitRecords.ApplyFilters(p);

        // Trước tiên lấy dữ liệu cơ bản vào bộ nhớ
        var records = await qs
            .Select(e => new { e.Ip, e.RequestPath, e.Time })
            .ToListAsync();

        // Xử lý phức tạp trong bộ nhớ
        // Nhóm theo IP và phiên
        var sessions = records
            .GroupBy(e => e.Ip)
            .Select(g => new {
                Ip = g.Key,
                Paths = g.OrderBy(e => e.Time).Select(e => e.RequestPath).ToList()
            })
            .ToList();

        // Tính tỷ lệ chuyển đổi
        var totalSessions = sessions.Count;
        var convertedSessions = sessions.Count(s => s.Paths.Contains(targetPath));
        var conversionRate = totalSessions == 0 ? 0 : (double)convertedSessions / totalSessions * 100;

        // Tính các trang vào hàng đầu
        var topEntryPages = sessions
            .Where(s => s.Paths.Count > 0)
            .GroupBy(s => s.Paths.First())
            .Select(g => new NameCountPair { Name = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Take(10)
            .ToList();

        return new ConversionStats {
            TargetPath = targetPath,
            TotalSessions = totalSessions,
            ConvertedSessions = convertedSessions,
            ConversionRate = conversionRate,
            TopEntryPages = topEntryPages
        };
    }
}
