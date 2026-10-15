using System.Text.Json;
using StarBlog.Data.Models;
using StarBlog.Web.Services;

namespace StarBlog.Web.Services.OutboxServices;

/// <summary>
/// Bộ xử lý tác vụ gửi email
/// </summary>
public class EmailSendOutboxHandler : IOutboxHandler {
    private readonly EmailService _emailService;

    public EmailSendOutboxHandler(EmailService emailService) {
        _emailService = emailService;
    }

    public string Type => OutboxTaskTypes.EmailSend;

    public async Task HandleAsync(OutboxMessage message, CancellationToken cancellationToken) {
        var payload = JsonSerializer.Deserialize<OutboxEmailPayload>(message.Payload);
        if (payload == null) throw new InvalidOperationException($"Outbox payload Giải tuần tự hóa thất bại: {message.Id}");

        await _emailService.SendEmailAsync(payload.Subject, payload.HtmlBody, payload.ToName, payload.ToAddress);
    }
}
