using StarBlog.Content.Utils;
using StarBlog.Application.Models.Config;

namespace StarBlog.Api.Extensions; 

public static class ConfigureAppSettings {
    public static void AddSettings(this IServiceCollection services, ConfigurationManager configuration) {
        configuration.AddJsonFile("appsettings-email.json", optional: true, reloadOnChange: true);
        configuration.AddJsonFile("appsettings-monitoring.json", optional: true, reloadOnChange: true);
        // Cấu hình bảo mật
        services.Configure<Auth>(configuration.GetSection(nameof(Auth)));
        // Cấu hình email
        services.Configure<EmailAccountConfig>(configuration.GetSection(nameof(EmailAccountConfig)));
        // Cấu hình giám sát
        services.Configure<Monitoring>(configuration.GetSection(nameof(Monitoring)));
    }
}
