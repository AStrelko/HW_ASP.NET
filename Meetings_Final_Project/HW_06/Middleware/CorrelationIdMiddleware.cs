using Serilog.Context;

namespace HW_06.Middleware;

/// <summary>
/// Додає унікальний CorrelationId
/// до кожного HTTP-запиту.
/// </summary>
public class CorrelationIdMiddleware
{
    private const string HeaderName = "X-Correlation-ID";

    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        ArgumentNullException.ThrowIfNull(next);
        _next = next;
    }

    /// <summary>
    /// Обробляє HTTP-запит та додає CorrelationId
    /// до відповіді й контексту логування.
    /// </summary>
    public async Task InvokeAsync(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var correlationId =
            context.Request.Headers.TryGetValue(
                HeaderName,
                out var existingCorrelationId)
            && !string.IsNullOrWhiteSpace(existingCorrelationId)
                ? existingCorrelationId.ToString()
                : Guid.NewGuid().ToString();

        context.Response.Headers[HeaderName] =
            correlationId;

        using (LogContext.PushProperty(
                   "CorrelationId",
                   correlationId))
        {
            await _next(context);
        }
    }
}