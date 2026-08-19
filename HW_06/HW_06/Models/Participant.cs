namespace HW_06.Models;

/// <summary>
/// Модель учасника зустрічей.
/// Містить профільні дані учасника та його зв'язки
/// з іншими сутностями предметної області.
/// </summary>
/// <remarks>
/// Participant не відповідає за автентифікацію та авторизацію.
/// Дані облікового запису зберігаються окремо в
/// <see cref="ApplicationUser"/>.
///
/// Учасник може існувати без облікового запису,
/// наприклад, якщо його було додано до зустрічі до реєстрації в системі.
/// </remarks>
public class Participant
{
    /// <summary>
    /// Унікальний ідентифікатор учасника.
    /// </summary>
    public int ParticipantId { get; set; }

    /// <summary>
    /// Ім'я учасника.
    /// </summary>
    public string FirstName { get; set; } = "";

    /// <summary>
    /// Прізвище учасника.
    /// </summary>
    public string LastName { get; set; } = "";

    /// <summary>
    /// Посада або спеціалізація учасника.
    /// </summary>
    public string? Position { get; set; }

    /// <summary>
    /// Назва файла аватарки учасника.
    /// </summary>
    public string? AvatarFileName { get; set; }

    /// <summary>
    /// Ідентифікатор облікового запису Identity,
    /// пов'язаного з учасником.
    /// </summary>
    public string? ApplicationUserId { get; set; }

    /// <summary>
    /// Обліковий запис користувача, пов'язаний з учасником.
    /// </summary>
    public ApplicationUser? ApplicationUser { get; set; }

    /// <summary>
    /// Колекція зв'язків між учасником та зустрічами.
    /// </summary>
    public ICollection<MeetingParticipant> MeetingParticipants { get; set; }
        = new List<MeetingParticipant>();

    /// <summary>
    /// Приватні файли, відправлені учасником.
    /// </summary>
    public ICollection<ParticipantPrivateFile> SentPrivateFiles { get; set; }
        = new List<ParticipantPrivateFile>();

    /// <summary>
    /// Приватні файли, отримані учасником.
    /// </summary>
    public ICollection<ParticipantPrivateFile> ReceivedPrivateFiles { get; set; }
        = new List<ParticipantPrivateFile>();
}