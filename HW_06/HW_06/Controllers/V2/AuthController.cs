using HW_06.DTOs.IdentityDTO;
using HW_06.Filters;
using HW_06.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HW_06.Controllers;

/// <summary>
/// Контролер для реєстрації та автентифікації користувачів.
/// </summary>
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    /// <summary>
    /// Ініціалізує контролер автентифікації.
    /// </summary>
    /// <param name="authService">
    /// Сервіс для реєстрації та входу користувачів.
    /// </param>
    public AuthController(IAuthService authService)
    {
        _authService = authService;
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
    [ServiceFilter(typeof(ValidationFilter<RegisterDTO>))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register(RegisterDTO dto, CancellationToken cancellationToken)
    {
        var result = await _authService.RegisterAsync(dto, cancellationToken);

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
    /// <returns>
    /// Результат автентифікації.
    /// </returns>
    [HttpPost("login")]
    [ServiceFilter(typeof(ValidationFilter<LoginDTO>))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(LoginDTO dto)
    {
        var result = await _authService.LoginAsync(dto);

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