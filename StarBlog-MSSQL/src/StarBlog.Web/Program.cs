using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using RobotsTxt;
using SixLabors.ImageSharp.Web.DependencyInjection;
using StarBlog.Application.Abstractions;
using StarBlog.Data;
using StarBlog.Data.Extensions;
using StarBlog.Web.Contrib.SiteMessage;
using StarBlog.Web.Extensions;
using StarBlog.Web.Filters;
using StarBlog.Web.Middlewares;
using StarBlog.Web.Services;
using StarBlog.Web.Services.BackgroundTasks;
using StarBlog.Web.Services.OutboxServices;

var builder = WebApplication.CreateBuilder(args);


var mvcBuilder = builder.Services.AddControllersWithViews(options => { options.Filters.Add<ResponseWrapperFilter>(); }
);

// Bật biên dịch động trang Razor ở chế độ phát triển
if (builder.Environment.IsDevelopment()) {
    mvcBuilder.AddRazorRuntimeCompilation();
}

builder.Services.AddMemoryCache();
builder.Services.AddHttpContextAccessor();

// Thêm nén phản hồi
builder.Services.AddResponseCompression(options => {
    options.EnableForHttps = true;

    // Cấu hình các compression provider (sắp xếp theo độ ưu tiên)
    options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.BrotliCompressionProvider>();
    options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.GzipCompressionProvider>();

    // Cấu hình các kiểu MIME cần nén
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

// Cấu hình mức nén Brotli
builder.Services.Configure<Microsoft.AspNetCore.ResponseCompression.BrotliCompressionProviderOptions>(options => {
    options.Level = System.IO.Compression.CompressionLevel.Optimal;
});

// Cấu hình mức nén Gzip
builder.Services.Configure<Microsoft.AspNetCore.ResponseCompression.GzipCompressionProviderOptions>(options => {
    options.Level = System.IO.Compression.CompressionLevel.Optimal;
});

builder.Services.AddSession(options => {
    options.IdleTimeout = TimeSpan.FromHours(1);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddAutoMapper(typeof(Program));
builder.Services.AddDbContext<AppDbContext>(options => {
    options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServer-Log"));
});
builder.Services.AddFreeSql(builder.Configuration);
builder.Services.AddVisitRecord();
builder.Services.AddHttpClient();
builder.Services.AddCors(options => {
    options.AddDefaultPolicy(policyBuilder => {
        policyBuilder.AllowCredentials();
        policyBuilder.AllowAnyHeader();
        policyBuilder.AllowAnyMethod();
        // policyBuilder.AllowAnyOrigin();
        policyBuilder.WithOrigins(
            "http://localhost:3000",
            "http://localhost:8080",
            "http://localhost:8081",
            "https://deali.cn",
            "https://blog.deali.cn"
        );
    });
});
builder.Services.AddStaticRobotsTxt(opt => {
    var baseUrl = builder.Configuration["host"] ?? "https://blog.deali.cn";
    opt.AddSection(section => section.AddUserAgent("Googlebot").Allow("/"))
        .AddSection(section => section.AddUserAgent("bingbot").Allow("/"))
        .AddSection(section => section.AddUserAgent("Bytespider").Disallow("/"))
        .AddSection(section => section.AddUserAgent("Sogou web spider").Allow("/"))
        .AddSection(section => section.AddUserAgent("*")
            .Disallow("/Admin/")
            .Disallow("/Api/")
            .Disallow("/bin/")
            .Disallow("/obj/")
            .Disallow("/node_modules/")
            .Disallow("/seo-test")
            .Allow("/"))
        .AddSitemap($"{baseUrl}/sitemap.xml")
        .AddSitemap($"{baseUrl}/sitemap-images.xml");

    return opt;
});
builder.Services.AddSwagger();
builder.Services.AddSettings(builder.Configuration);
builder.Services.AddAuth(builder.Configuration);
// Đăng ký IHttpClientFactory, tham khảo: https://docs.microsoft.com/zh-cn/dotnet/core/extensions/http-client
builder.Services.AddHttpClient();
builder.Services.AddImageSharp();
// Đăng ký các dịch vụ tùy chỉnh
builder.Services.AddSingleton<CommonService>();
builder.Services.AddSingleton<EmailService>();
builder.Services.AddSingleton<MessageService>();
builder.Services.AddSingleton<ThemeService>();
builder.Services.AddSingleton<TempFilterService>();
builder.Services.AddSingleton<MonitoringService>();
builder.Services.AddSingleton<IBackgroundTaskQueue, StarBlog.Application.Services.BackgroundTaskQueue>();
builder.Services.AddScoped<BlogService>();
builder.Services.AddScoped<CategoryService>();
builder.Services.AddScoped<CommentService>();
builder.Services.AddScoped<ConfigService>();
builder.Services.AddScoped<LinkExchangeService>();
builder.Services.AddScoped<LinkService>();
builder.Services.AddScoped<PhotoService>();
builder.Services.AddScoped<PostService>();
builder.Services.AddScoped<SeoService>();
builder.Services.AddScoped<StructuredDataService>();
builder.Services.AddScoped<ImageSeoService>();
builder.Services.AddScoped<SitemapService>();
builder.Services.AddScoped<TranslationService>();

// Cấu hình dịch thuật
builder.Services.Configure<TranslationConfig>(builder.Configuration.GetSection(TranslationConfig.SectionName));

builder.Services.Configure<OutboxOptions>(builder.Configuration.GetSection("Outbox"));
builder.Services.AddScoped<OutboxService>();
builder.Services.AddScoped<OutboxProcessor>();
builder.Services.AddScoped<IOutboxHandler, EmailSendOutboxHandler>();
builder.Services.AddHostedService<OutboxWorker>();
builder.Services.AddHostedService<BackgroundTaskWorker>();

// Đặt kích thước tối đa của request
builder.WebHost.ConfigureKestrel(options => { options.Limits.MaxRequestBodySize = long.MaxValue; });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) {
    app.UseDeveloperExceptionPage();
}
else {
    app.UseExceptionHandler(applicationBuilder => {
        applicationBuilder.Run(async context => {
            // todo Ghi nhật ký lỗi
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsJsonAsync(new { message = "Unexpected error!" });
        });
    });
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseForwardedHeaders(new ForwardedHeadersOptions {
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

app.UseImageSharp();
// app.UseHttpsRedirection();

// Bật nén phản hồi
app.UseResponseCompression();

// Thêm middleware gỡ lỗi tệp tĩnh (chỉ ở môi trường không phải production)
if (!app.Environment.IsProduction()) {
    app.UseMiddleware<StaticFileDebugMiddleware>();
}

// Cấu hình bộ nhớ đệm cho tệp tĩnh
app.UseStaticFiles(new StaticFileOptions {
    ServeUnknownFileTypes = true,
    OnPrepareResponse = ctx => {
        var path = ctx.Context.Request.Path.Value;

        // Với tệp JS và CSS, đặt thời gian cache ngắn hơn để tiện gỡ lỗi
        if (path != null && (path.EndsWith(".js") || path.EndsWith(".css"))) {
            if (app.Environment.IsDevelopment()) {
                // Môi trường phát triển không cache JS/CSS
                ctx.Context.Response.Headers.CacheControl = "no-cache, no-store, must-revalidate";
                ctx.Context.Response.Headers.Pragma = "no-cache";
                ctx.Context.Response.Headers.Expires = "0";
            }
            else {
                // Môi trường production cache ngắn hạn JS/CSS
                const int shortDuration = 60 * 60 * 24; // 1 ngày
                ctx.Context.Response.Headers.CacheControl = $"public,max-age={shortDuration}";
                ctx.Context.Response.Headers.Expires = DateTime.UtcNow.AddDays(1).ToString("R");
            }
        }
        else {
            // Các tệp tĩnh khác cache dài hạn
            const int durationInSeconds = 60 * 60 * 24 * 30; // 30 ngày
            ctx.Context.Response.Headers.CacheControl = $"public,max-age={durationInSeconds}";
            ctx.Context.Response.Headers.Expires = DateTime.UtcNow.AddDays(30).ToString("R");
        }
    }
});

app.UseMiddleware<VisitRecordMiddleware>();
app.UseRobotsTxt();
app.UseRouting();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.UseSession();

app.UseSwaggerPkg();

app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

public partial class Program { }
