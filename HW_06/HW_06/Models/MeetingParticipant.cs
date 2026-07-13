namespace HW_06.Models;

/// <summary>
/// Проміжна сутність для зв'язку "багато-до-багатьох"
/// між зустрічами та учасниками.
/// </summary>
public class MeetingParticipant
{
    /// <summary>
    /// Ідентифікатор учасника.
    /// </summary>
    public int ParticipantId { get; set; }

    /// <summary>
    /// Ідентифікатор зустрічі.
    /// </summary>
    public int MeetingId { get; set; }

    /// <summary>
    /// Навігаційна властивість до учасника.
    /// </summary>
    public Participant Participant { get; set; } = null!;

    /// <summary>
    /// Навігаційна властивість до зустрічі.
    /// </summary>
    public Meeting Meeting { get; set; } = null!;
}