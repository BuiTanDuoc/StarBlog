using System.Reflection;
using ClinicManagement.Application.Common.Behaviors;
using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Services;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace ClinicManagement.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        services.AddAutoMapper(confg => { } , assembly);
        services.AddValidatorsFromAssembly(assembly);

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);

            // Order matters: exception logging wraps everything, then validation runs
            // before the handler ever executes, then per-request timing.
            cfg.AddOpenBehavior(typeof(UnhandledExceptionBehavior<,>));
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
            cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
        });

        services.AddScoped<ISubscriptionLimitService, SubscriptionLimitService>();
        services.AddScoped<IAppointmentAvailabilityService, AppointmentAvailabilityService>();
        services.AddScoped<IAuthTokenIssuer, AuthTokenIssuer>();
        services.AddScoped<IDrugSafetyChecker, DrugSafetyChecker>();

        return services;
    }
}
