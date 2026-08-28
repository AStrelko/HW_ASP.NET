using Microsoft.AspNetCore.Mvc;

namespace HW_06.Middleware;

/// <summary>
/// Middleware режиму технічного обслуговування.
/// Якщо в конфігурації Maintenance:Enabled = true,
/// повертає 503 Service Unavailable та припиняє виконання конвеєра.
/// </summary>
public class MaintenanceMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IConfiguration _configuration;

    /// <summary>
    /// Ініціалізує middleware технічного обслуговування.
    /// </summary>
    /// <param name="next">
    /// Наступний middleware у конвеєрі.
    /// </param>
    /// <param name="configuration">
    /// Конфігурація застосунку.
    /// </param>
    public MaintenanceMiddleware(
        RequestDelegate next,
        IConfiguration configuration)
    {
        _next = next;
        _configuration = configuration;
    }

    /// <summary>
    /// Перевіряє, чи увімкнено режим
    /// технічного обслуговування.
    /// </summary>
    public async Task InvokeAsync(HttpContext context)
    {
        var maintenanceEnabled =
            _configuration.GetValue<bool>(
                "Maintenance:Enabled");

        if (maintenanceEnabled)
        {
            context.Response.StatusCode =
                StatusCodes.Status503ServiceUnavailable;

            var problemDetails = new ProblemDetails
            {
                Status =
                    StatusCodes.Status503ServiceUnavailable,

                Title =
                    "Service Unavailable",

                Detail =
                    "Сервіс тимчасово недоступний через технічне обслуговування.",

                Instance =
                    context.Request.Path
            };

            await context.Response.WriteAsJsonAsync(
                problemDetails);

            return;
        }

        await _next(context);
    }
}