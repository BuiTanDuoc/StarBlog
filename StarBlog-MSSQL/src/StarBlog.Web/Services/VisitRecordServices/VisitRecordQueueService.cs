using System.Collections.Concurrent;
using AutoMapper;
using IP2Region.Net.Abstractions;
using StarBlog.Data;
using StarBlog.Data.Models;
using UAParser;

namespace StarBlog.Web.Services.VisitRecordServices;

public class VisitRecordQueueService {
    private readonly ConcurrentQueue<VisitRecord> _logQueue = new ConcurrentQueue<VisitRecord>();
    private readonly ILogger<VisitRecordQueueService> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ISearcher _searcher;
    private readonly IMapper _mapper;
    private readonly Parser _uaParser = Parser.GetDefault();

    /// <summary>
    /// Kích thước lô
    /// </summary>
    private const int BatchSize = 10;

    public VisitRecordQueueService(ILogger<VisitRecordQueueService> logger, IServiceScopeFactory scopeFactory,
        ISearcher searcher, IMapper mapper) {
        _logger = logger;
        _scopeFactory = scopeFactory;
        _searcher = searcher;
        _mapper = mapper;
    }

    // Đưa nhật ký vào hàng đợi
    public void EnqueueLog(VisitRecord log) {
        _logQueue.Enqueue(log);
    }

    // Phương thức ghi hàng loạt vào cơ sở dữ liệu theo định kỳ
    public async Task WriteLogsToDatabaseAsync(CancellationToken cancellationToken) {
        if (_logQueue.IsEmpty) {
            // Tạm thời chờ, tránh các lần kiểm tra vô nghĩa với tần suất cao
            await Task.Delay(1000, cancellationToken);
            return;
        }

        var batch = new List<VisitRecord>();
        // Lấy một lô nhật ký từ hàng đợi
        while (_logQueue.TryDequeue(out var log) && batch.Count < BatchSize) {
            log = InflateIpRegion(log);
            log = InflateUA(log);
            batch.Add(log);
        }

        try {
            using var scope = _scopeFactory.CreateScope();
            var dbCtx = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await using var transaction = await dbCtx.Database.BeginTransactionAsync(cancellationToken);
            try {
                dbCtx.VisitRecords.AddRange(batch);
                await dbCtx.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                _logger.LogInformation("Nhật ký truy cập Successfully wrote {BatchCount} logs to the database", batch.Count);
            }
            catch (Exception) {
                await transaction.RollbackAsync(cancellationToken);
                // Ném lại ngoại lệ để đảm bảo bên ngoài bắt được
                throw;
            }
        }
        catch (Exception ex) {
            _logger.LogError(ex, "Nhật ký truy cập Error writing logs to the database: {ExMessage}", ex.Message);
        }
    }

    private VisitRecord InflateIpRegion(VisitRecord log) {
        if (string.IsNullOrWhiteSpace(log.Ip)) return log;

        var result = _searcher.Search(log.Ip);
        if (string.IsNullOrWhiteSpace(result)) return log;

        var parts = result.Split('|');
        log.IpInfo = new IpInfo {
            Country = parts[0],
            RegionCode = parts[1],
            Province = parts[2],
            City = parts[3],
            Isp = parts[4]
        };

        return log;
    }

    /// <summary>
    /// Phân tích thông tin user agent
    /// </summary>
    /// <param name="log">Bản ghi truy cập</param>
    /// <returns>Bản ghi truy cập đã được điền thông tin user agent</returns>
    private VisitRecord InflateUA(VisitRecord log) {
        // Kiểm tra UserAgent có phải null hoặc chuỗi rỗng không, tránh UAParser ném ArgumentNullException
        if (string.IsNullOrWhiteSpace(log.UserAgent)) {
            // Nếu UserAgent rỗng, đặt UserAgentInfo về giá trị mặc định
            log.UserAgentInfo = new UserAgentInfo();
            return log;
        }
        
        var c = _uaParser.Parse(log.UserAgent);
        log.UserAgentInfo = _mapper.Map<UserAgentInfo>(c);
        if (!string.IsNullOrWhiteSpace(log.UserAgentInfo.UserAgent.Family)
            && log.UserAgentInfo.UserAgent.Family.ToLower().Contains("bytespider")) {
            log.UserAgentInfo.Device.IsSpider = true;
        }
        return log;
    }
}