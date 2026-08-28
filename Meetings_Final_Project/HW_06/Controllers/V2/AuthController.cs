using HW_06.DTOs.IdentityDTO;
using HW_06.Features.Auth.Commands.Login;
using HW_06.Features.Auth.Commands.Register;
using MediatR;
using Microsoft.AspNetCore.Mvc;

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
    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register(RegisterDTO dto, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new RegisterCommand(dto), cancellationToken);

        if (!result.Succeeded)
        {
            return BadRequest(result.Errors);
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
    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(LoginDTO dto, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new LoginCommand(dto), cancellationToken);

        if (!result.Success)
        {
            return Unauthorized(new ProblemDetails
                {
                    Title = result.Message,
                    Status = StatusCodes.Status401Unauthorized
                });
        }

        return Ok(result.Response);
    }
}