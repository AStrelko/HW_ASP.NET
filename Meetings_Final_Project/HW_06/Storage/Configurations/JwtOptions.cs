namespace HW_06.Storage.Configurations;

/// <summary>
/// Налаштування JWT-токенів.
/// </summary>
public class JwtOptions
{
    /// <summary>
    /// Назва секції JWT у конфігурації.
    /// </summary>
    public const string SectionName =
        "Jwt";

    /// <summary>
    /// Видавець токена.
    /// </summary>
    public string Issuer { get; set; }
        = string.Empty;

    /// <summary>
    /// Аудиторія токена.
    /// </summary>
    public string Audience { get; set; }
        = string.Empty;

    /// <summary>
    /// Секретний ключ для підпису JWT.
    /// </summary>
    public string Key { get; set; }
        = string.Empty;

    /// <summary>
    /// Час життя access token у хвилинах.
    /// </summary>
    public int AccessTokenMinutes { get; set; }
        = 60;
}