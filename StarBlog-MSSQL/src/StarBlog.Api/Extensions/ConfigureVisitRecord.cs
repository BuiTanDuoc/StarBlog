using IP2Region.Net.Abstractions;
using IP2Region.Net.XDB;
using StarBlog.Api.Services.VisitRecordServices;
using StarBlog.Infrastructure.Ip;
using StarBlog.Application.Services.VisitRecordServices;

namespace StarBlog.Api.Extensions;

public static class ConfigureVisitRecord {
    public static IServiceCollection AddVisitRecord(this IServiceCollection services) {
        services.AddScoped<VisitRecordAnalyticsService>();
        services.AddSingleton<VisitRecordQueueService>();
        services.AddHostedService<VisitRecordWorker>();
        
        var dbPath = Path.Combine(Environment.CurrentDirectory, "ip2region.xdb");
        if (File.Exists(dbPath)) {
            services.AddSingleton<ISearcher>(new Searcher(
                // Ở đây chọn cache toàn bộ tệp dữ liệu vào bộ nhớ, hiệu năng cao hơn và cũng đảm bảo thread-safe
                // Thực tế bản triển khai C# không cache cũng thread-safe (theo lời tác giả)
                CachePolicy.Content, dbPath
            ));
        }
        else {
            services.AddSingleton<ISearcher, FakeIpSearcher>();
        }
        
        return services;
    }
}
