using ClinicManagement.Domain.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagement.API.Common;

/// <summary>
/// Every controller inherits this. [Authorize] is the default — actions
/// that should be public (login, plan listing, tenant signup) opt out
/// individually with [AllowAnonymous]. Response envelope is unified here:
/// success returns the raw Data (200), failure returns { succeeded, errors }
/// (400) — Result/Result&lt;T&gt; already carry exactly this shape, so this is
/// a thin mapping, not a wrapper on top of a wrapper.
/// </summary>
[ApiController]
[Authorize]
[Route("api/[controller]")]
public abstract class ApiControllerBase : ControllerBase
{
    private ISender? _mediator;
    protected ISender Mediator => _mediator ??= HttpContext.RequestServices.GetRequiredService<ISender>();

    /// <summary>Client IP, preferring X-Forwarded-For (set by a reverse proxy/load balancer) over the raw connection address.</summary>
    protected string? ClientIpAddress =>
        Request.Headers.TryGetValue("X-Forwarded-For", out var forwarded) && forwarded.Count > 0
            ? forwarded[0]?.Split(',')[0].Trim()
            : HttpContext.Connection.RemoteIpAddress?.ToString();

    protected IActionResult HandleResult<T>(Result<T> result) =>
        result.Succeeded ? Ok(result.Data) : BadRequest(new { succeeded = false, errors = result.Errors });

    protected IActionResult HandleResult(Result result) =>
        result.Succeeded ? Ok(new { succeeded = true }) : BadRequest(new { succeeded = false, errors = result.Errors });

    /// <summary>201 Created variant for POST endpoints that create a new resource.</summary>
    protected IActionResult HandleCreatedResult<T>(Result<T> result, string actionName, object? routeValues) =>
        result.Succeeded
            ? CreatedAtAction(actionName, routeValues, result.Data)
            : BadRequest(new { succeeded = false, errors = result.Errors });
}
