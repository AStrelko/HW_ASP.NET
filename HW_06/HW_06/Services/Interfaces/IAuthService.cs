using HW_06.DTOs.IdentityDTO;
using Microsoft.AspNetCore.Identity;

namespace HW_06.Services.Interfaces;

/// <summary>
/// Визначає операції реєстрації та входу користувачів.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Реєструє нового користувача
    /// та створює пов'язаний профіль учасника.
    /// </summary>
    Task<IdentityResult> RegisterAsync(RegisterDTO dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Виконує вхід користувача в систему.
    /// </summary>
    Task<LoginResult> LoginAsync(LoginDTO dto);
}