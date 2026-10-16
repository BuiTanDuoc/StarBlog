using System.Net;
using System.Text.Json;
using ClinicManagement.Domain.Exceptions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.API.Middleware;

/// <summary>
/// Sits at the very front of the pipeline (registered first in
/// Program.cs) so it catches everything downstream, including auth and
/// routing failures that make it past the framework. Handlers throw
/// domain-specific exceptions for exceptional cases (limits exceeded,
/// scheduling conflicts) and this is the single place that decides what
/// HTTP status and shape the client sees — no controller needs its own
/// try/catch.
/// </summary>
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger, IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            await HandleExceptionAsync(context, exception);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, errors) = MapException(exception);

        if (statusCode == HttpStatusCode.InternalServerError)
        {
            // Full detail goes to the log; the client only ever sees a generic message
            // (avoids leaking stack traces / connection strings / internal type names).
            _logger.LogError(exception, "Unhandled exception processing {Method} {Path}",
                context.Request.Method, context.Request.Path);
        }
        else
        {
            _logger.LogWarning("Handled exception ({StatusCode}) processing {Method} {Path}: {Message}",
                (int)statusCode, context.Request.Method, context.Request.Path, exception.Message);
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var payload = new
        {
            succeeded = false,
            errors,
            // Stack traces only in Development — never in Staging/Production responses.
            stackTrace = _environment.IsDevelopment() ? exception.StackTrace : null
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(payload, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        }));
    }

    private static (HttpStatusCode StatusCode, string[] Errors) MapException(Exception exception) => exception switch
    {
        ValidationException validationEx => (
            HttpStatusCode.BadRequest,
            validationEx.Errors.Select(e => $"{e.PropertyName}: {e.ErrorMessage}").ToArray()),

        SubscriptionLimitExceededException limitEx => (HttpStatusCode.PaymentRequired, new[] { limitEx.Message }),

        AppointmentConflictException conflictEx => (HttpStatusCode.Conflict, new[] { conflictEx.Message }),

        TenantMismatchException tenantEx => (HttpStatusCode.Forbidden, new[] { tenantEx.Message }),

        DomainException domainEx => (HttpStatusCode.BadRequest, new[] { domainEx.Message }),

        UnauthorizedAccessException => (HttpStatusCode.Unauthorized, new[] { "You are not authorized to perform this action." }),

        KeyNotFoundException => (HttpStatusCode.NotFound, new[] { exception.Message }),

        // Covers FK-restrict violations (e.g. deleting a Branch that still has Doctors or
        // Appointments pointing at it) and unique-index collisions (e.g. a race on the
        // human-friendly PatientCode/InvoiceNumber generation — see the comments on
        // GenerateNextPatientCodeAsync/GenerateNextInvoiceNumberAsync). Neither is a 500;
        // both are "the request conflicts with existing data," which is what 409 means.
        DbUpdateException => (HttpStatusCode.Conflict,
            new[] { "This action conflicts with existing data — related records may still reference this item, or a duplicate value was submitted." }),

        _ => (HttpStatusCode.InternalServerError, new[] { "An unexpected error occurred. Please try again later." })
    };
}

public static class GlobalExceptionMiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandling(this IApplicationBuilder app) =>
        app.UseMiddleware<GlobalExceptionMiddleware>();
}
