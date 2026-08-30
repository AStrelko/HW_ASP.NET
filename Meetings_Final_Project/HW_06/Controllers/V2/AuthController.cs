using System.Diagnostics;
using HW_06.DTOs.IdentityDTO;
using HW_06.Features.Auth.Commands.Login;
using HW_06.Features.Auth.Commands.Register;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace HW_06.Controllers;

/// <summary>
/// Контролер для реєстрації
/// та автентифікації користувачів.
/// </summary>
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly ISender _sender;

    /// <summary>
    /// Ініціалізує контролер автентифікації.
    /// </summary>
    /// <param name="sender">
    /// Сервіс MediatR для надсилання команд і запитів.
    /// </param>
    public AuthController(ISender sender)
    {
        ArgumentNullException.ThrowIfNull(sender);

        _sender = sender;
    }

    /// <summary>
    /// Реєструє нового користувача
    /// та створює пов'язаний профіль учасника.
    /// </summary>
    /// <param name="dto">
    /// Дані нового користувача.
    /// </param>
    /// <param name="cancellationToken">
    /// Токен скасування операції.
    /// </param>
    /// <returns>
    /// Результат реєстрації.
    /// </returns>
    /// <response code="200">
    /// Користувача успішно зареєстровано.
    /// </response>
    /// <response code="400">
    /// Дані реєстрації не пройшли перевірку.
    /// </response>
    /// <response code="429">
    /// Перевищено ліміт запитів.
    /// </response>
    [HttpPost("register")]
    [EnableRateLimiting("Register")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> Register([FromBody] RegisterDTO dto, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new RegisterCommand(dto), cancellationToken);

        if (!result.Succeeded)
        {
            var errors = result.Errors
                    .GroupBy(error => GetRegistrationField(error.Code))
                    .ToDictionary(group => group.Key, group => group
                            .Select(error => error.Description)
                            .Distinct()
                            .ToArray());

            var problem = new ValidationProblemDetails(errors)
                {
                    Type = "https://www.rfc-editor.org/rfc/rfc9110#section-15.5.1",
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Помилка реєстрації.",
                    Detail = "Перевірте введені дані та повторіть спробу.",
                    Instance = Request.Path
                };

            return CreateProblemResponse(problem);
        }

        return Ok(new
        {
            Message = "Користувача успішно зареєстровано."
        });
    }

    /// <summary>
    /// Виконує вхід зареєстрованого користувача.
    /// </summary>
    /// <param name="dto">
    /// Дані для входу користувача.
    /// </param>
    /// <param name="cancellationToken">
    /// Токен скасування операції.
    /// </param>
    /// <returns>
    /// Результат автентифікації.
    /// </returns>
    /// <response code="200">
    /// Вхід успішний. Повертається JWT access token.
    /// </response>
    /// <response code="400">
    /// Дані для входу не пройшли валідацію.
    /// </response>
    /// <response code="401">
    /// Не вдалося виконати вхід.
    /// </response>
    /// <response code="429">
    /// Перевищено ліміт запитів.
    /// </response>
    [HttpPost("login")]
    [EnableRateLimiting("Login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> Login([FromBody] LoginDTO dto, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new LoginCommand(dto), cancellationToken);

        if (!result.Success)
        {
            var problem = new ProblemDetails
                {
                    Type = "https://www.rfc-editor.org/rfc/rfc9110#section-15.5.2",
                    Status = StatusCodes.Status401Unauthorized,
                    Title = "Не вдалося виконати вхід.",
                    Detail = "Неправильний email або пароль, або вхід тимчасово недоступний.",
                    Instance = Request.Path
                };

            return CreateProblemResponse(problem);
        }

        return Ok(result.Response);
    }

    /// <summary>
    /// Зіставляє код помилки Identity
    /// з полем форми реєстрації.
    /// </summary>
    private static string GetRegistrationField(string errorCode)
    {
        // У нашому проєкті UserName дорівнює Email.
        if (errorCode is
            "DuplicateEmail" or
            "InvalidEmail" or
            "DuplicateUserName" or
            "InvalidUserName")
        {
            return nameof(RegisterDTO.Email);
        }

        if (errorCode.StartsWith("Password", StringComparison.Ordinal))
        {
            return nameof(RegisterDTO.Password);
        }

        return "Registration";
    }

    /// <summary>
    /// Додає ідентифікатор трасування
    /// та формує відповідь application/problem+json.
    /// </summary>
    private ObjectResult CreateProblemResponse(ProblemDetails problem)
    {
        problem.Extensions["traceId"] = Activity.Current?.Id ?? HttpContext.TraceIdentifier;

        return new ObjectResult(problem)
        {
            StatusCode = problem.Status, ContentTypes =
            {
                "application/problem+json"
            }
        };
    }
}