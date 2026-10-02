using System.Net;
using System.Text.Json;
using Api.Application.Common.Exceptions;
using FluentValidation;

namespace Api.Middleware;

public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    { _next = next; _logger = logger; }

    public async Task Invoke(HttpContext ctx)
    {
        try { await _next(ctx); }
        catch (NotFoundException ex)
        {
            ctx.Response.StatusCode = (int)HttpStatusCode.NotFound;
            ctx.Response.ContentType = "application/json";
            await ctx.Response.WriteAsync(JsonSerializer.Serialize(new { error = ex.Message }));
        }
        catch (ValidationException ex)
        {
            ctx.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            ctx.Response.ContentType = "application/json";
            var errors = ex.Errors.GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
            await ctx.Response.WriteAsync(JsonSerializer.Serialize(new { error = "Validation failed", errors }));
        }
        catch (ExternalServiceException ex)
        {
            _logger.LogError(ex, "External service failure");
            ctx.Response.StatusCode = (int)HttpStatusCode.BadGateway; // 502 — external dependency failed
            ctx.Response.ContentType = "application/json";
            await ctx.Response.WriteAsync(JsonSerializer.Serialize(new { error = ex.Message }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");
            ctx.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            ctx.Response.ContentType = "application/json";
            await ctx.Response.WriteAsync(JsonSerializer.Serialize(new { error = "An unexpected error occurred." }));
        }
    }
}
