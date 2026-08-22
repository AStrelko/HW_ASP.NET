namespace HW_06.Models;

public class Meeting
{
    /// <summary>
    /// Id зустрічі.
    /// </summary>
    public int MeetingId { get; set; }
    /// <summary>
    /// Назва зустрічі.
    /// </summary>
    public string Title { get; set; } = string.Empty;
    /// <summary>
    /// Опис зустрічі.
    /// </summary>
    public string Description { get; set; } = string.Empty;
    /// <summary>
    /// Дата та час зустрічі.
    /// </summary>
    public DateTime DateTime { get; set; }
    /// <summary>
    /// Id кімнати зустрічі.
    /// </summary>
    public int? RoomId { get; set; }
    /// <summary>
    /// Номер кімнати зустрічі.
    /// </summary>
    public Room? Room { get; set; } 
    /// <summary>
    /// Ідентифікатор користувача,
    /// який організував зустріч.
    /// </summary>
    public string OrganizerId { get; set; } = string.Empty;
    /// <summary>
    /// Учасники зустрічі.
    /// </summary>
    public ICollection<MeetingParticipant> MeetingParticipants { get; set; }
        = new List<MeetingParticipant>();
    /// <summary>
    /// Коллекция файлов-вложений, прикреплённых к встрече.
    /// </summary>
    public ICollection<MeetingAttachment> Attachments { get; set; }
        = new List<MeetingAttachment>();
}