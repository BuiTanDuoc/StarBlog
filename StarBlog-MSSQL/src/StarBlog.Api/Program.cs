using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp.Web.DependencyInjection;
using StarBlog.Application.Abstractions;
using StarBlog.Api.Filters;
using StarBlog.Api.Adapters;
using StarBlog.Api.Services.BackgroundTasks;
using StarBlog.Api.Services.OutboxServices;
using StarBlog.Application.Services;
using StarBlog.Application.Services.OutboxServices;
using StarBlog.Data;
using StarBlog.Data.Extensions;
using StarBlog.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);

// WebAPI thuần: chỉ đăng ký Controllers (không bao gồm MVC Views)
// Đồng thời bật bao bọc phản hồi toàn cục, đảm bảo cấu trúc trả về của hầu hết Action ổn định và nhất quán (ApiResponse / ApiResponsePaged).
builder.Services.AddControllers(options => {
    options.Filters.Add<ResponseWrapperFilter>();
});

// Cache và HttpContextAccessor: một số dịch vụ (ví dụ thống kê, phân tích IP/UA, logic email/bình luận) sẽ dùng đến
builder.Services.AddMemoryCache();
builder.Services.AddHttpContextAccessor();

// Nén phản hồi: bật Brotli/Gzip cho các phản hồi JSON/XML/văn bản tĩnh, v.v. để giảm tiêu thụ băng thông
builder.Services.AddResponseCompression(options => {
    options.EnableForHttps = true;
    options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.BrotliCompressionProvider>();
    options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.GzipCompressionProvider>();
    options.MimeTypes = Microsoft.AspNetCore.ResponseCompression.ResponseCompressionDefaults.MimeTypes.Concat(new[] {
        "application/javascript",
        "application/json",
        "application/xml",
        "text/css",
        "text/html",
        "text/json",
        "text/plain",
        "text/xml"
    });
});

builder.Services.Configure<Microsoft.AspNetCore.ResponseCompression.BrotliCompressionProviderOptions>(options => {
    options.Level = System.IO.Compression.CompressionLevel.Optimal;
});

builder.Services.Configure<Microsoft.AspNetCore.ResponseCompression.GzipCompressionProviderOptions>(options => {
    options.Level = System.IO.Compression.CompressionLevel.Optimal;
});

// AutoMapper: ánh xạ DTO <-> Entity của controller
builder.Services.AddAutoMapper(typeof(Program));

// EF Core SQL Server dùng cho nhật ký truy cập/thống kê (cùng tồn tại với cơ sở dữ liệu nghiệp vụ FreeSql)
builder.Services.AddDbContext<AppDbContext>(options => {
    options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServer-Log"));
});

// Truy cập dữ liệu nghiệp vụ (FreeSql)
builder.Services.AddFreeSql(builder.Configuration);
builder.Services.AddVisitRecord();
builder.Services.AddHttpClient();

// HealthChecks: cung cấp các endpoint /health (tổng hợp), /health/live (còn sống), /health/ready (sẵn sàng)
builder.Services.AddStarBlogHealthChecks();

// CORS: cần cho frontend Next.js gọi chéo miền (khi kèm Cookie/thông tin xác thực thì phải liệt kê rõ các Origin được phép)
builder.Services.AddCors(options => {
    options.AddDefaultPolicy(policyBuilder => {
        policyBuilder.AllowCredentials();
        policyBuilder.AllowAnyHeader();
        policyBuilder.AllowAnyMethod();
        policyBuilder.WithOrigins(
            "http://localhost:3000",
            "http://localhost:8080",
            "http://localhost:8081",
            "https://deali.cn",
            "https://blog.deali.cn"
        );
    });
});

// Swagger: xuất OpenAPI theo các nhóm hiện có, đồng thời giữ lại middleware xác thực truy cập Swagger (tùy chọn)
builder.Services.AddSwagger();

// AppSettings: nạp các cấu hình liên quan đến email/giám sát/bảo mật (trong đó tệp json của email/monitoring là optional)
builder.Services.AddSettings(builder.Configuration);

// JWT Bearer: các API quản trị phụ thuộc vào
builder.Services.AddAuth(builder.Configuration);

// ImageSharp: giữ nguyên khả năng xử lý hình ảnh hiện có (ví dụ các dịch vụ và chuỗi middleware liên quan đến hình ảnh)
builder.Services.AddImageSharp();

builder.Services.AddSingleton<IAppPathProvider, AspNetAppPathProvider>();
builder.Services.AddSingleton<IFileStorage, PhysicalFileStorage>();
builder.Services.AddSingleton<IClock, SystemClock>();
builder.Services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();

// Dịch vụ tầng ứng dụng: để đảm bảo hành vi API giống với StarBlog.Web, đợt di chuyển đầu tiên dùng nguyên danh sách Service hiện có
builder.Services.AddSingleton<CommonService>();
builder.Services.AddSingleton<EmailService>();
builder.Services.AddSingleton<ThemeService>();
builder.Services.AddSingleton<TempFilterService>();
builder.Services.AddSingleton<MonitoringService>();
builder.Services.AddScoped<BlogService>();
builder.Services.AddScoped<CategoryService>();
builder.Services.AddScoped<CommentService>();
builder.Services.AddScoped<ConfigService>();
builder.Services.AddScoped<LinkExchangeService>();
builder.Services.AddScoped<LinkService>();
builder.Services.AddScoped<PhotoService>();
builder.Services.AddScoped<PostService>();

// Outbox: bất đồng bộ hóa các "tác vụ cần xử lý nền" (ví dụ gửi email), tránh chặn phản hồi của API
builder.Services.Configure<OutboxOptions>(builder.Configuration.GetSection("Outbox"));
builder.Services.AddScoped<OutboxService>();
builder.Services.AddScoped<OutboxProcessor>();
builder.Services.AddScoped<IOutboxHandler, EmailSendOutboxHandler>();
builder.Services.AddHostedService<OutboxWorker>();
builder.Services.AddHostedService<BackgroundTaskWorker>();

// Các tình huống tải lên/nhập có thể chứa tệp lớn, nên nới giới hạn kích thước request body của Kestrel (do logic nghiệp vụ tự kiểm tra)
builder.WebHost.ConfigureKestrel(options => {
    options.Limits.MaxRequestBodySize = long.MaxValue;
});

var app = builder.Build();

if (app.Environment.IsDevelopment()) {
    app.UseDeveloperExceptionPage();
}
else {
    app.UseExceptionHandler(applicationBuilder => {
        applicationBuilder.Run(async context => {
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsJsonAsync(new { message = "Unexpected error!" });
        });
    });
    app.UseHsts();
}

app.UseForwardedHeaders(new ForwardedHeadersOptions {
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

// Ghi chú: middleware ImageSharp cần có webroot (dự án này cung cấp thư mục wwwroot rỗng để đáp ứng việc khởi động)
app.UseImageSharp();
app.UseResponseCompression();

app.UseRouting();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

// Swagger UI: môi trường Development truy cập trực tiếp không cần đăng nhập (sau khi khởi động trình duyệt tự mở /api-docs/swagger),
// các môi trường khác vẫn cần người dùng đã xác thực mới truy cập được (tránh để lộ tài liệu trực tiếp ở môi trường production)
app.UseSwaggerPkg(requireAuth: !app.Environment.IsDevelopment());

app.MapStarBlogHealthChecks();

app.MapControllers();

app.Run();

public partial class Program { }
