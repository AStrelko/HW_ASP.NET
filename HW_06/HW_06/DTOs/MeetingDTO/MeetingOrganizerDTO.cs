namespace HW_06.DTOs.MeetingDTO;

/// <summary>
/// Коротка інформація
/// про організатора зустрічі.
/// </summary>
public record MeetingOrganizerDTO(
    int ParticipantId,
    string FirstName,
    string LastName);