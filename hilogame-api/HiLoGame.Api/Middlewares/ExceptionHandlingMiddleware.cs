using HiLoGame.Application.Common.Exceptions;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace HiLoGame.Api.Middlewares;

public sealed class ExceptionHandlingMiddleware : IMiddleware
{
    private readonly ProblemDetailsFactory _pdf;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    public ExceptionHandlingMiddleware(ProblemDetailsFactory pdf, ILogger<ExceptionHandlingMiddleware> logger)
        => (_pdf, _logger) = (pdf, logger);

    public async Task InvokeAsync(HttpContext ctx, RequestDelegate next)
    {
        try
        {
            await next(ctx);
        }
        catch (AppException ex)
        {
            // Map semantic exceptions to HTTP
            var (status, title) = ex switch
            {
                ValidationException => (StatusCodes.Status422UnprocessableEntity, "Validation failed"),
                NotFoundException   => (StatusCodes.Status404NotFound, "Resource not found"),
                ConflictException   => (StatusCodes.Status409Conflict, "Conflict"),
                ForbiddenException  => (StatusCodes.Status403Forbidden, "Forbidden"),
                UnauthorizedException => (StatusCodes.Status401Unauthorized, "Unauthorized"),
                _ => (StatusCodes.Status400BadRequest, "Bad request")
            };

            var pd = _pdf.CreateProblemDetails(
                ctx,
                statusCode: status,
                title: title,
                detail: ex.Message,
                type: $"https://httpstatuses.io/{status}");

            // Put machine-readable error code + metadata into extensions (RFC7807-friendly)
            pd.Extensions["code"] = ex.Code;

            // Useful for support/tracing
            pd.Extensions["traceId"] = ctx.TraceIdentifier;

            // Structured logging (don’t dump validation noise as Error)
            var level = ex is ValidationException ? LogLevel.Information : LogLevel.Warning;
            _logger.Log(level, ex, "Handled {Code} -> {Status}", ex.Code, status);

            ctx.Response.StatusCode = status;
            ctx.Response.ContentType = "application/problem+json";
            await ctx.Response.WriteAsJsonAsync(pd);
        }
        catch (DbUpdateConcurrencyException dbcx)
        {
            var pd = _pdf.CreateProblemDetails(
                ctx,
                statusCode: StatusCodes.Status409Conflict,
                title: "Concurrency conflict",
                detail: "The resource was modified by another process. Retry with the latest version.",
                type: "https://httpstatuses.io/409");

            pd.Extensions["code"] = "concurrency_conflict";
            pd.Extensions["traceId"] = ctx.TraceIdentifier;

            _logger.LogWarning(dbcx, "EF concurrency conflict");
            ctx.Response.StatusCode = StatusCodes.Status409Conflict;
            ctx.Response.ContentType = "application/problem+json";
            await ctx.Response.WriteAsJsonAsync(pd);
        }
    }
}