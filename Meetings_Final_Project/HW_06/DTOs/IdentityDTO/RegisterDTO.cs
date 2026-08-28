namespace HW_06.DTOs.IdentityDTO;

/// <summary>
/// Дані для реєстрації нового користувача в системі.
/// </summary>
/// <remarks>
/// Під час реєстрації на основі цих даних створюється
/// обліковий запис <c>ApplicationUser</c> та пов'язаний
/// з ним профіль <c>Participant</c>.
/// </remarks>
public class RegisterDTO
{
    /// <summary>
    /// Електронна адреса користувача.
    /// Використовується як логін облікового запису.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Пароль користувача.
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Підтвердження пароля.
    /// </summary>
    public string ConfirmPassword { get; set; } = string.Empty;

    /// <summary>
    /// Ім'я учасника.
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Прізвище учасника.
    /// </summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Посада або спеціалізація учасника.
    /// </summary>
    public string? Position { get; set; }
}