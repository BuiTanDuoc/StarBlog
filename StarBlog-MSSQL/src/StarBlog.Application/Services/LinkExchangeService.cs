using System.Text;
using FreeSql;
using StarBlog.Data.Models;
using StarBlog.Application.Services.OutboxServices;

namespace StarBlog.Application.Services;

/// <summary>
/// Yêu cầu trao đổi liên kết bạn bè
/// </summary>
public class LinkExchangeService {
    private readonly IBaseRepository<LinkExchange> _repo;
    private readonly LinkService _linkService;
    private readonly OutboxService _outboxService;

    public LinkExchangeService(IBaseRepository<LinkExchange> repo, LinkService linkService, OutboxService outboxService) {
        _repo = repo;
        _linkService = linkService;
        _outboxService = outboxService;
    }

    /// <summary>
    /// Kiểm tra id có tồn tại hay không
    /// </summary>
    public async Task<bool> HasId(int id) {
        return await _repo.Where(a => a.Id == id).AnyAsync();
    }

    public async Task<bool> HasUrl(string url) {
        return await _repo.Where(a => a.Url.Contains(url)).AnyAsync();
    }

    public async Task<List<LinkExchange>> GetAll() {
        return await _repo.Select.ToListAsync();
    }

    public async Task<LinkExchange?> GetById(int id) {
        return await _repo.Where(a => a.Id == id).FirstAsync();
    }

    public async Task<LinkExchange> AddOrUpdate(LinkExchange item) {
        return await _repo.InsertOrUpdateAsync(item);
    }

    public async Task<LinkExchange?> SetVerifyStatus(int id, bool status, string? reason = null) {
        var item = await GetById(id);
        if (item == null) return null;

        item.Verified = status;
        item.Reason = reason;
        await _repo.UpdateAsync(item);


        var link = await _linkService.GetByName(item.Name);
        if (status) {
            await SendEmailOnAccept(item);
            if (link == null) {
                await _linkService.AddOrUpdate(new Link {
                    Name = item.Name,
                    Description = item.Description,
                    Url = item.Url,
                    Visible = true
                });
            }
            else {
                await _linkService.SetVisibility(link.Id, true);
            }
        }
        else {
            await SendEmailOnReject(item);
            if (link != null) await _linkService.DeleteById(link.Id);
        }

        return await GetById(id);
    }

    public async Task<int> DeleteById(int id) {
        return await _repo.DeleteAsync(a => a.Id == id);
    }

    public async Task SendEmail(LinkExchange item, string subject, string message) {
        var sb = new StringBuilder();
        sb.AppendLine($"<p>{message}</p>");
        sb.AppendLine($"<br>");
        sb.AppendLine($"<p>Dưới đây là thông tin liên kết bạn bè bạn đã đăng ký:</p>");
        sb.AppendLine($"<p>Tên website: {item.Name}</p>");
        sb.AppendLine($"<p>Giới thiệu: {item.Description}</p>");
        sb.AppendLine($"<p>URL: {item.Url}</p>");
        sb.AppendLine($"<p>Quản trị viên website: {item.WebMaster}</p>");
        if (item.Reason != null) sb.AppendLine($"<p>Thông tin bổ sung: {item.Reason}</p>");
        sb.AppendLine($"<br>");
        sb.AppendLine($"<br>");
        await _outboxService.EnqueueEmailAsync(
            $"[StarBlog]{subject}",
            sb.ToString(),
            item.WebMaster,
            item.Email,
            dedupKey: $"link-exchange:{item.Id}:{subject}"
        );
    }

    public async Task SendEmailOnAdd(LinkExchange item) {
        await SendEmail(item, "Yêu cầu trao đổi liên kết đã được gửi", "Yêu cầu trao đổi liên kết đã được gửi, đang được xử lý, vui lòng theo dõi thông báo qua email~");
    }

    public async Task SendEmailOnAccept(LinkExchange item) {
        await SendEmail(item, "Phản hồi kết quả yêu cầu trao đổi liên kết", "Xin chào, yêu cầu trao đổi liên kết đã được chấp nhận! Cảm ơn bạn đã ủng hộ, hoan nghênh ghé thăm lẫn nhau nhé~");
    }

    public async Task SendEmailOnReject(LinkExchange item) {
        await SendEmail(item, "Phản hồi kết quả yêu cầu trao đổi liên kết", "Rất tiếc, yêu cầu trao đổi liên kết không được chấp nhận! Đề nghị bạn xem thông tin bổ sung, điều chỉnh rồi đăng ký lại, cảm ơn sự thông cảm và ủng hộ của bạn~");
    }
}
