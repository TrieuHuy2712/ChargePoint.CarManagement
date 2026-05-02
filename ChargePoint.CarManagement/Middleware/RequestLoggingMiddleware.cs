using System.Diagnostics;
using ChargePoint.CarManagement.Models;
using ChargePoint.CarManagement.Services.Logging;

namespace ChargePoint.CarManagement.Middleware;

public class RequestLoggingMiddleware(RequestDelegate next)
{
    private readonly RequestDelegate _next = next;

    public async Task InvokeAsync(HttpContext context, ILogStore logStore)
    {
        if (ShouldSkip(context.Request.Path))
        {
            await _next(context);
            return;
        }

        var sw = Stopwatch.StartNew();
        Exception? capturedException = null;

        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            capturedException = ex;
            throw;
        }
        finally
        {
            sw.Stop();

            var statusCode = capturedException == null
                ? context.Response.StatusCode
                : StatusCodes.Status500InternalServerError;

            var entry = new SystemLogEntry
            {
                TimestampUtc = DateTime.UtcNow,
                Level = capturedException == null ? "Information" : "Error",
                Source = "Request",
                Message = $"{context.Request.Method} {context.Request.Path} => {statusCode} ({sw.ElapsedMilliseconds}ms)",
                Detail = capturedException?.ToString(),
                UserName = context.User.Identity?.IsAuthenticated == true
                    ? context.User.Identity?.Name
                    : "Anonymous",
                TraceId = context.TraceIdentifier
            };

            await logStore.WriteAsync(entry, CancellationToken.None);
        }
    }

    private static bool ShouldSkip(PathString path)
    {
        var value = path.Value?.ToLowerInvariant();
        if (string.IsNullOrEmpty(value))
        {
            return true;
        }

        if (value.StartsWith("/lib") ||
            value.StartsWith("/css") ||
            value.StartsWith("/js") ||
            value.StartsWith("/images") ||
            value.StartsWith("/favicon"))
        {
            return true;
        }

        return value.EndsWith(".png") || value.EndsWith(".jpg") || value.EndsWith(".jpeg")
            || value.EndsWith(".gif") || value.EndsWith(".ico") || value.EndsWith(".svg")
            || value.EndsWith(".webp");
    }
}
