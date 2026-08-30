using System.Diagnostics;
using System.Text.Json;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace HW_06.Middleware;

/// <summary>
/// Глобально обробляє винятки HTTP-запитів
/// та формує відповіді у форматі ProblemDetails.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;

    private readonly ILogger<ExceptionHandlingMiddleware>
        _logger;

    /// <summary>
    /// Ініціалізує middleware глобальної обробки винятків.
    /// </summary>
    /// <param name="next">
    /// Наступний middleware у конвеєрі.
    /// </param>
    /// <param name="logger">
    /// Сервіс журналювання помилок.
    /// </param>
    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
    {
        ArgumentNullException.ThrowIfNull(next);
        ArgumentNullException.ThrowIfNull(logger);

        _next = next;
        _logger = logger;
    }

    /// <summary>
    /// Виконує наступний компонент конвеєра
    /// та перехоплює необроблені винятки.
    /// </summary>
    /// <param name="context">
    /// Поточний HTTP-контекст.
    /// </param>
    public async Task InvokeAsync(
        HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        try
        {
            await _next(context);
        }
        catch (OperationCanceledException)
            when (context.RequestAborted.IsCancellationRequested)
        {
            // Клієнт скасував запит.
            // Не намагаємося надсилати йому JSON-відповідь.
            if (!context.Response.HasStarted)
            {
                context.Response.StatusCode =
                    StatusCodes.Status499ClientClosedRequest;
            }
        }
        catch (Exception exception)
            when (context.Response.HasStarted)
        {
            _logger.LogError(
                exception,
                "Помилка після початку надсилання відповіді "
                + "для запиту {Method} {Path}.",
                context.Request.Method,
                context.Request.Path);

            // Заголовки або частину тіла вже надіслано.
            // Замінити відповідь на ProblemDetails неможливо.
            throw;
        }
        catch (ValidationException exception)
        {
            _logger.LogWarning(
                "Помилка валідації під час виконання "
                + "запиту {Method} {Path}.",
                context.Request.Method,
                context.Request.Path);

            await HandleValidationExceptionAsync(
                context,
                exception);
        }
        catch (ArgumentException exception)
        {
            _logger.LogWarning(
                "Некоректний запит {Method} {Path}. "
                + "Причина: {Message}",
                context.Request.Method,
                context.Request.Path,
                exception.Message);

            await HandleExceptionAsync(
                context,
                StatusCodes.Status400BadRequest,
                "Некоректний запит.",
                exception.Message);
        }
        catch (KeyNotFoundException exception)
        {
            _logger.LogWarning(
                "Ресурс не знайдено під час виконання "
                + "запиту {Method} {Path}. Причина: {Message}",
                context.Request.Method,
                context.Request.Path,
                exception.Message);

            await HandleExceptionAsync(
                context,
                StatusCodes.Status404NotFound,
                "Ресурс не знайдено.",
                exception.Message);
        }
        catch (BadHttpRequestException exception)
        {
            _logger.LogWarning(
                "Помилка HTTP-запиту {Method} {Path}. "
                + "StatusCode: {StatusCode}",
                context.Request.Method,
                context.Request.Path,
                exception.StatusCode);

            if (exception.StatusCode ==
                StatusCodes.Status413PayloadTooLarge)
            {
                await HandleExceptionAsync(
                    context,
                    StatusCodes.Status413PayloadTooLarge,
                    "Запит занадто великий.",
                    "Розмір тіла запиту перевищує "
                    + "допустиме значення.");
            }
            else
            {
                await HandleExceptionAsync(
                    context,
                    exception.StatusCode,
                    "Некоректний HTTP-запит.",
                    "Сервер не може обробити "
                    + "переданий HTTP-запит.");
            }
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Виникла необроблена помилка "
                + "під час виконання запиту {Method} {Path}.",
                context.Request.Method,
                context.Request.Path);

            await HandleExceptionAsync(
                context,
                StatusCodes.Status500InternalServerError,
                "Внутрішня помилка сервера.",
                "Під час виконання запиту "
                + "виникла непередбачена помилка.");
        }
    }

    /// <summary>
    /// Формує відповідь для помилок FluentValidation.
    /// </summary>
    private static async Task HandleValidationExceptionAsync(
        HttpContext context,
        ValidationException exception)
    {
        var failures =
            exception.Errors.ToList();

        Dictionary<string, string[]> errors;

        if (failures.Count > 0)
        {
            errors =
                failures
                    .GroupBy(error =>
                        GetFieldName(error.PropertyName))
                    .ToDictionary(
                        group => group.Key,
                        group => group
                            .Select(error =>
                                error.ErrorMessage)
                            .Distinct()
                            .ToArray());
        }
        else
        {
            errors =
                new Dictionary<string, string[]>
                {
                    ["Validation"] =
                    [
                        exception.Message
                    ]
                };
        }

        var problem =
            new ValidationProblemDetails(errors)
            {
                Status =
                    StatusCodes.Status400BadRequest,

                Title =
                    "Помилка валідації.",

                Detail =
                    "Передані дані не пройшли перевірку.",

                Instance =
                    context.Request.Path
            };

        await WriteProblemAsync(
            context,
            problem);
    }

    /// <summary>
    /// Формує відповідь для інших оброблених винятків.
    /// </summary>
    private static async Task HandleExceptionAsync(
        HttpContext context,
        int statusCode,
        string title,
        string detail)
    {
        var problem =
            new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail = detail,
                Instance = context.Request.Path
            };

        await WriteProblemAsync(
            context,
            problem);
    }

    /// <summary>
    /// Додає тип помилки й traceId
    /// та записує JSON-відповідь.
    /// </summary>
    private static async Task WriteProblemAsync<TProblem>(
        HttpContext context,
        TProblem problem)
        where TProblem : ProblemDetails
    {
        var statusCode =
            problem.Status
            ?? StatusCodes.Status500InternalServerError;

        problem.Status =
            statusCode;

        problem.Type =
            statusCode switch
            {
                StatusCodes.Status400BadRequest =>
                    "https://www.rfc-editor.org/rfc/rfc9110#section-15.5.1",

                StatusCodes.Status404NotFound =>
                    "https://www.rfc-editor.org/rfc/rfc9110#section-15.5.5",

                StatusCodes.Status413PayloadTooLarge =>
                    "https://www.rfc-editor.org/rfc/rfc9110#section-15.5.14",

                StatusCodes.Status500InternalServerError =>
                    "https://www.rfc-editor.org/rfc/rfc9110#section-15.6.1",

                _ => "about:blank"
            };

        problem.Extensions["traceId"] =
            Activity.Current?.Id
            ?? context.TraceIdentifier;

        context.Response.StatusCode =
            statusCode;

        // Прибираємо можливу довжину попередньої відповіді.
        context.Response.ContentLength =
            null;

        await context.Response.WriteAsJsonAsync(
            problem,
            options: (JsonSerializerOptions?)null,
            contentType: "application/problem+json",
            cancellationToken: context.RequestAborted);
    }

    /// <summary>
    /// Прибирає технічний префікс Dto
    /// з назви поля помилки.
    /// </summary>
    private static string GetFieldName(
        string? propertyName)
    {
        if (string.IsNullOrWhiteSpace(propertyName))
        {
            return "Validation";
        }

        return propertyName.StartsWith(
                "Dto.",
                StringComparison.Ordinal)
            ? propertyName[4..]
            : propertyName;
    }
}