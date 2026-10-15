using System.Text.Json;
using FreeSql;
using Microsoft.Extensions.Options;
using StarBlog.Data.Models;

namespace StarBlog.Web.Services.OutboxServices;

/// <summary>
/// Outbox Dịch vụ đưa vào hàng đợi
/// <para>Chịu trách nhiệm ghi các "tác vụ cần xử lý bất đồng bộ" vào bảng outbox_message của cơ sở dữ liệu chính.</para>
/// </summary>
public class OutboxService {
    private readonly ILogger<OutboxService> _logger;
    private readonly IBaseRepository<OutboxMessage> _outboxRepo;
    private readonly OutboxOptions _options;

    public OutboxService(ILogger<OutboxService> logger, IBaseRepository<OutboxMessage> outboxRepo, IOptions<OutboxOptions> options) {
        _logger = logger;
        _outboxRepo = outboxRepo;
        _options = options.Value;
    }

    /// <summary>
    /// Đưa một tác vụ Outbox chung vào hàng đợi
    /// <para>payloadJson Nên là chuỗi JSON, thuận tiện cho việc mở rộng và điều tra sau này.</para>
    /// </summary>
    public async Task<long> EnqueueAsync(
        string type,
        string payloadJson,
        string? dedupKey = null,
        int? maxAttempts = null,
        DateTime? nextAttemptAt = null
    ) {
        var now = NormalizeTime(DateTime.Now);
        var entity = new OutboxMessage {
            Type = type,
            DedupKey = dedupKey,
            Payload = payloadJson,
            Status = OutboxStatus.Pending,
            Attempt = 0,
            MaxAttempts = maxAttempts ?? _options.DefaultMaxAttempts,
            NextAttemptAt = NormalizeTime(nextAttemptAt ?? now),
            CreatedAt = now,
            UpdatedAt = now,
        };

        await _outboxRepo.InsertAsync(entity);
        _logger.LogInformation("Outbox Đưa vào hàng đợi: {Type} #{Id}, DedupKey: {DedupKey}", entity.Type, entity.Id, entity.DedupKey);
        return entity.Id;
    }

    /// <summary>
    /// Đưa một tác vụ "gửi email" vào hàng đợi
    /// <para>Lưu ý: Phương thức này chỉ chịu trách nhiệm lưu vào cơ sở dữ liệu, không chịu trách nhiệm gửi ngay lập tức.</para>
    /// </summary>
    public async Task<long> EnqueueEmailAsync(
        string subject,
        string htmlBody,
        string toName,
        string toAddress,
        string? dedupKey = null,
        int? maxAttempts = null,
        DateTime? nextAttemptAt = null
    ) {
        var payload = new OutboxEmailPayload {
            Subject = subject,
            HtmlBody = htmlBody,
            ToName = toName,
            ToAddress = toAddress,
        };

        return await EnqueueAsync(
            OutboxTaskTypes.EmailSend,
            JsonSerializer.Serialize(payload),
            dedupKey,
            maxAttempts,
            nextAttemptAt
        );
    }

    private static DateTime NormalizeTime(DateTime value) {
        return new DateTime(value.Year, value.Month, value.Day, value.Hour, value.Minute, value.Second, value.Kind);
    }
}
