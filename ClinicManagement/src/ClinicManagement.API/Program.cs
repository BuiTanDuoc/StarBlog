using System.Text;
using System.Text.Json.Serialization;
using AspNetCoreRateLimit;
using ClinicManagement.API.Hubs;
using ClinicManagement.API.Middleware;
using ClinicManagement.API.Services;
using ClinicManagement.Application;
using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Infrastructure;
using ClinicManagement.Infrastructure.BackgroundJobs;
using ClinicManagement.Infrastructure.Options;
using ClinicManagement.Persistence;
using ClinicManagement.Persistence.Seed;
using Hangfire;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// ---------- Serilog ----------
// Reads sinks/levels from appsettings.json ("Serilog" section) if present; falls back to console.
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
    .WriteTo.Console());

// ---------- MVC / Swagger ----------
// Enums (Gender, AppointmentStatus, InvoiceStatus, etc.) serialize/deserialize as their
// string names (e.g. "Male") rather than raw ints — the frontend sends/reads string names
// everywhere, so without this every enum-bearing request/response would break.
builder.Services.AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Clinic Management SaaS API", Version = "v1" });

    var jwtScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Paste only the raw JWT — Swagger adds the \"Bearer \" prefix for you.",
        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
    };
    options.AddSecurityDefinition("Bearer", jwtScheme);
    options.AddSecurityRequirement(new OpenApiSecurityRequirement { [jwtScheme] = Array.Empty<string>() });
});

// ---------- Application / Persistence / Infrastructure ----------
builder.Services.AddApplication();
builder.Services.AddPersistence(builder.Configuration);
builder.Services.AddInfrastructure(builder.Configuration); // registers the custom IAuthorizationPolicyProvider too

builder.Services.AddSignalR();
builder.Services.AddScoped<INotificationDispatcher, SignalRNotificationDispatcher>();

// ---------- Authentication (JWT) ----------
var jwtSettings = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
    ?? throw new InvalidOperationException("Jwt configuration section is missing.");

builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtSettings.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Convert.FromBase64String(jwtSettings.Secret)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1)
        };

        // Browsers can't set an Authorization header on the WebSocket upgrade request that
        // starts a SignalR connection, so the client passes the token as ?access_token=...
        // instead — this reads it from there specifically for hub requests, nowhere else.
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                if (!string.IsNullOrEmpty(accessToken) &&
                    context.HttpContext.Request.Path.StartsWithSegments("/hubs"))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });

// ---------- Authorization ----------
// Note: AddInfrastructure() above already registered PermissionPolicyProvider as the
// IAuthorizationPolicyProvider, which supersedes the default one AddAuthorization sets up here.
builder.Services.AddAuthorization();

// ---------- CORS ----------
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("Default", policy => policy
        .WithOrigins(allowedOrigins)
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials());
});

// ---------- Rate limiting ----------
builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
builder.Services.Configure<IpRateLimitPolicies>(builder.Configuration.GetSection("IpRateLimitPolicies"));
builder.Services.AddInMemoryRateLimiting();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

// ---------- Health checks ----------
builder.Services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>();

var app = builder.Build();

// ---------- Startup: migrate + seed ----------
// Safe to run on every boot: MigrateAsync no-ops if already current, and every seeder is idempotent.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await db.Database.MigrateAsync();
}
await app.Services.SeedDatabaseAsync();

// ---------- Middleware pipeline ----------
// Order matters. Exception handling wraps everything; rate limiting and CORS run before
// auth so a rejected/cross-origin request never reaches business logic; the subscription
// guard runs after auth (it needs the JWT's tenant_id claim) but before controllers.
app.UseGlobalExceptionHandling();
app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseIpRateLimiting();
app.UseCors("Default");

app.UseAuthentication();
app.UseAuthorization();
app.UseSubscriptionGuard();

app.MapControllers();
app.MapHub<NotificationHub>("/hubs/notifications");
app.MapHealthChecks("/health");

// ---------- Hangfire dashboard + recurring jobs ----------
// Dashboard has no [Authorize] filter configured here — lock this down (e.g.
// DashboardOptions.Authorization) before deploying anywhere but localhost.
app.UseHangfireDashboard("/hangfire");
using (var scope = app.Services.CreateScope())
{
    var recurringJobManager = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();
    recurringJobManager.ScheduleRecurringJobs(scope.ServiceProvider);
}

app.Run();
