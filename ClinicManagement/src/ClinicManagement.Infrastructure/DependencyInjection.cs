using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Infrastructure.Authorization;
using ClinicManagement.Infrastructure.BackgroundJobs;
using ClinicManagement.Infrastructure.FileStorage;
using ClinicManagement.Infrastructure.Identity;
using ClinicManagement.Infrastructure.Options;
using ClinicManagement.Infrastructure.Reporting;
using ClinicManagement.Infrastructure.Services;
using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ClinicManagement.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
        services.AddScoped<ITokenService, TokenService>();

        services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
        services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

        services.AddHangfire(config => config
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UseSqlServerStorage(configuration.GetConnectionString("DefaultConnection")));
        services.AddHangfireServer();
        services.AddScoped<ExpiredSubscriptionsJob>();
        services.AddScoped<AppointmentReminderJob>();
        services.AddScoped<OverdueInvoicesJob>();
        services.AddScoped<IExportService, ExportService>();
        services.AddScoped<IFileStorageService, LocalFileStorageService>();

        services.Configure<ReminderSettings>(configuration.GetSection(ReminderSettings.SectionName));
        services.Configure<SmtpSettings>(configuration.GetSection(SmtpSettings.SectionName));
        services.Configure<TwilioSettings>(configuration.GetSection(TwilioSettings.SectionName));
        services.Configure<StripeSettings>(configuration.GetSection(StripeSettings.SectionName));
        services.AddHttpClient();
        services.AddScoped<IEmailSender, Notifications.SmtpEmailSender>();
        services.AddScoped<ISmsSender, Notifications.TwilioSmsSender>();
        services.AddScoped<IWhatsAppSender, Notifications.TwilioWhatsAppSender>();
        services.AddScoped<IPaymentGatewayService, Payments.StripePaymentGatewayService>();

        return services;
    }
}
