namespace HW_06.Common.Constants;

/// <summary>
/// Містить ролі користувачів,
/// що використовуються в застосунку.
/// </summary>
public static class ApplicationRoles
{
    /// <summary>
    /// Роль адміністратора.
    /// </summary>
    public const string Admin = "Admin";

    /// <summary>
    /// Роль звичайного користувача.
    /// </summary>
    public const string User = "User";

    /// <summary>
    /// Усі доступні ролі користувачів.
    /// </summary>
    public static readonly string[] All =
    [
        Admin,
        User
    ];
}