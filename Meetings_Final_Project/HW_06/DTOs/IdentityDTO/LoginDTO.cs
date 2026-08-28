namespace HW_06.DTOs.IdentityDTO;

/// <summary>
/// Дані для входу користувача в систему.
/// </summary>
public class LoginDTO
{
    /// <summary>
    /// Електронна адреса зареєстрованого користувача.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Пароль користувача.
    /// </summary>
    public string Password { get; set; } = string.Empty;
}