using System.Diagnostics;
using ClinicManagement.Application.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Application.Common.Behaviors;

/// <summary>Logs every request with timing; flags anything over 500ms as a slow-request warning.</summary>
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;
    private readonly ICurrentUserService _currentUserService;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger, ICurrentUserService currentUserService)
    {
        _logger = logger;
        _currentUserService = currentUserService;
    }

    public async Task<TResponse> Handle(
        TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var stopwatch = Stopwatch.StartNew();

        _logger.LogInformation(
            "Handling {RequestName} for TenantId {TenantId}, UserId {UserId}",
            requestName, _currentUserService.TenantId, _currentUserService.UserId);

        var response = await next();

        stopwatch.Stop();
        if (stopwatch.ElapsedMilliseconds > 500)
        {
            _logger.LogWarning(
                "Slow request: {RequestName} took {ElapsedMilliseconds}ms", requestName, stopwatch.ElapsedMilliseconds);
        }

        return response;
    }
}

/// <summary>Catches anything a handler doesn't handle itself and logs it with full request context before rethrowing.</summary>
public class UnhandledExceptionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<UnhandledExceptionBehavior<TRequest, TResponse>> _logger;

    public UnhandledExceptionBehavior(ILogger<UnhandledExceptionBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        try
        {
            return await next();
        }
        catch (Exception ex) when (ex is not FluentValidation.ValidationException)
        {
            _logger.LogError(ex, "Unhandled exception for request {RequestName} {@Request}", typeof(TRequest).Name, request);
            throw;
        }
    }
}
