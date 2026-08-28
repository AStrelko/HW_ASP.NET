using Microsoft.AspNetCore.Identity;

namespace HW_06.Models;

/// <summary>
/// Модель зареєстрованого користувача системи.
/// Розширює стандартну модель ASP.NET Core Identity.
/// </summary>
/// <remarks>
/// ApplicationUser відповідає за дані автентифікації та авторизації:
/// логін, email, пароль, ролі, токени та інші Identity-дані.
///
/// Дані, що належать безпосередньо учаснику зустрічей
/// (ім'я, прізвище, посада, аватар, зустрічі та файли),
/// зберігаються окремо в сутності <see cref="Participant"/>.
///
/// Між ApplicationUser та Participant використовується зв'язок один-до-одного.
/// </remarks>
public class ApplicationUser : IdentityUser
{
    /// <summary>
    /// Профіль учасника, пов'язаний з обліковим записом користувача.
    /// </summary>
    public Participant? Participant { get; set; }
}