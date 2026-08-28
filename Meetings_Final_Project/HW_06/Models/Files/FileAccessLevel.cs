namespace HW_06.Models.Files;

/// <summary>
/// Визначає рівень доступу до файлу.
/// </summary>
public enum FileAccessLevel
{
    /// <summary>
    /// Публічний файл, доступний без додаткової авторизації.
    /// </summary>
    Public,

    /// <summary>
    /// Приватний файл, доступ до якого мають лише авторизовані користувачі.
    /// </summary>
    Private
}