using System.Text;
using MailKit;
using Microsoft.Extensions.Options;
using StarBlog.Content.Utils;

namespace StarBlog.Web.Services;

public class EmailService {
    private readonly ILogger<EmailService> _logger;
    private readonly EmailAccountConfig _emailAccountConfig;
    private const string StarblogLink = "<a href=\"https://deali.cn\">StarBlog</a>";


    public EmailService(ILogger<EmailService> logger, IOptions<EmailAccountConfig> options) {
        _logger = logger;
        _emailAccountConfig = options.Value;
    }

    public async Task<MessageSentEventArgs> SendEmailAsync(string subject, string body, string toName, string toAddress) {
        _logger.LogDebug("Gửi email, chủ đề: {Subject}, người nhận: {ToAddress}", subject, toAddress);
        body += $"<br><p>Tin nhắn này được gửi tự động bởi {StarblogLink}, không cần trả lời.</p>";
        return await EmailUtils.SendEmailAsync(_emailAccountConfig, subject, body, toName, toAddress);
    }

    /// <summary>
    /// Gửi mã xác thực email
    /// <returns>Sinh mã xác thực ngẫu nhiên</returns>
    /// <param name="mock">Chỉ sinh mã xác thực, không gửi email</param>
    /// </summary>
    public async Task<string> SendOtpMail(string email, bool mock = false) {
        var otp = Random.Shared.NextInt64(1000, 9999).ToString();

        var sb = new StringBuilder();
        sb.AppendLine($"<p>Chào mừng bạn đến với StarBlog! Mã xác thực: {otp}</p>");
        sb.AppendLine($"<p>Nếu bạn không thực hiện bất kỳ thao tác nào, vui lòng bỏ qua email này.</p>");

        if (!mock) {
            await SendEmailAsync(
                "[StarBlog] Mã xác thực email",
                sb.ToString(),
                email,
                email
            );
        }

        return otp;
    }
}
