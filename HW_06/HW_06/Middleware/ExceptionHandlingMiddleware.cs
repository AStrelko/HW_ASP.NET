using HW_06.Validators.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace HW_06.Middleware;

/// <summary>
/// Глобально обробляє винятки,
/// що виникають під час виконання HTTP-запитів,
/// та перетворює їх на коректні HTTP-відповіді
/// у форматі ProblemDetails.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

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
        try
        {
            await _next(context);
        }
        catch (ValidationException exception)
        {
            await HandleExceptionAsync(
                context,
                StatusCodes.Status400BadRequest,
                "Помилка валідації",
                exception.Message);
        }
        catch (ArgumentException exception)
        {
            await HandleExceptionAsync(
                context,
                StatusCodes.Status400BadRequest,
                "Некоректний запит",
                exception.Message);
        }
        catch (KeyNotFoundException exception)
        {
            await HandleExceptionAsync(
                context,
                StatusCodes.Status404NotFound,
                "Ресурс не знайдено",
                exception.Message);
        }
        catch (OperationCanceledException)
            when (context.RequestAborted.IsCancellationRequested)
        {
            // Запит було скасовано клієнтом.
            // Це не є внутрішньою помилкою сервера.
        }
        
        catch (BadHttpRequestException exception)
            when (exception.StatusCode ==
                  StatusCodes.Status413PayloadTooLarge)
        {
            await HandleExceptionAsync(
                context,
                StatusCodes.Status413PayloadTooLarge,
                "Файл занадто великий",
                "Розмір завантажуваного файлу перевищує допустиме значення.");
        }
        
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Виникла необроблена помилка під час виконання запиту.");

            await HandleExceptionAsync(
                context,
                StatusCodes.Status500InternalServerError,
                "Внутрішня помилка сервера",
                "Під час виконання запиту виникла непередбачена помилка.");
        }
    }

    /// <summary>
    /// Формує HTTP-відповідь
    /// у форматі <see cref="ProblemDetails"/>.
    /// </summary>
    /// <param name="context">
    /// Поточний HTTP-контекст.
    /// </param>
    /// <param name="statusCode">
    /// HTTP-код відповіді.
    /// </param>
    /// <param name="title">
    /// Короткий опис типу помилки.
    /// </param>
    /// <param name="detail">
    /// Детальний опис помилки.
    /// </param>
    private static async Task HandleExceptionAsync(
        HttpContext context,
        int statusCode,
        string title,
        string detail)
    {
        var problemDetails =
            new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail = detail,
                Instance = context.Request.Path
            };

        context.Response.StatusCode =
            statusCode;

        context.Response.ContentType =
            "application/problem+json";

        await context.Response.WriteAsJsonAsync(
            problemDetails);
    }
}