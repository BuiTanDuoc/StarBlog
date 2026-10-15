using System.Diagnostics;
using StarBlog.Data.Models;
using StarBlog.Web.Extensions;
using StarBlog.Web.Services.VisitRecordServices;

namespace StarBlog.Web.Middlewares;

public class VisitRecordMiddleware {
    private readonly RequestDelegate _next;

    public VisitRecordMiddleware(RequestDelegate requestDelegate) {
        _next = requestDelegate;
    }

    public async Task Invoke(HttpContext context, VisitRecordQueueService logQueue) {
        var request = context.Request;

        // Ghi lại thời điểm bắt đầu
        var stopwatch = Stopwatch.StartNew();

        // Tạo trước một đối tượng rỗng, điền dữ liệu sau
        var record = new VisitRecord {
            Ip = context.GetRemoteIpAddress()?.ToString(),
            RequestPath = request.Path,
            RequestQueryString = request.QueryString.Value,
            RequestMethod = request.Method,
            UserAgent = request.Headers.UserAgent,
            Time = DateTime.Now,
            Referrer = request.Headers.Referer, // Trang nguồn
        };

        // Trước khi phản hồi thực sự bắt đầu ghi tới client, bổ sung mã trạng thái và thời gian xử lý
        context.Response.OnStarting(state => {
            var (rec, sw, queue) = (Tuple<VisitRecord, Stopwatch, VisitRecordQueueService>)state;
            // Kết thúc bấm giờ
            sw.Stop();

            rec.StatusCode = context.Response.StatusCode;
            rec.ResponseTimeMs = (int)sw.ElapsedMilliseconds;

            // Đưa vào hàng đợi
            queue.EnqueueLog(rec);
            return Task.CompletedTask;
        }, Tuple.Create(record, stopwatch, logQueue));

        // Thực thi các middleware tiếp theo trong pipeline
        await _next(context);
    }
}