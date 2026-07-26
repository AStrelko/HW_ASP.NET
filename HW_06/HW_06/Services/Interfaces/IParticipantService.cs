using HW_06.DTOs.MeetingDTO;
using HW_06.DTOs.ParticipantDTO;
using HW_06.Helpers.Pagination;
using HW_06.Helpers.QueryParameters;

namespace HW_06.Services.Interfaces;

/// <summary>
/// Сервіс для роботи з учасниками.
/// Забезпечує створення, отримання, оновлення,
/// видалення та пошук учасників.
/// </summary>
public interface IParticipantService
{
    /// <summary>
    /// Отримати список учасників
    /// з підтримкою пагінації, пошуку та сортування.
    /// </summary>
    Task<PagedResult<ParticipantReadDTO>> GetAllAsync(
        ParticipantQueryParameters parameters);

    /// <summary>
    /// Отримати учасника за його ідентифікатором.
    /// </summary>
    Task<ParticipantDetailDTO?> GetByIdAsync(int id);

    /// <summary>
    /// Створити нового учасника.
    /// </summary>
    Task<ParticipantReadDTO> CreateAsync(
        ParticipantCreateDTO dto);

    /// <summary>
    /// Повністю оновити інформацію про учасника.
    /// </summary>
    Task<bool> UpdateAsync(
        int id,
        ParticipantUpdateDTO dto);

    /// <summary>
    /// Частково оновити інформацію про учасника.
    /// </summary>
    Task<bool> PartialUpdateAsync(
        int id,
        ParticipantPartialUpdateDTO dto);

    /// <summary>
    /// Видалити учасника.
    /// </summary>
    Task<bool> DeleteAsync(int id);

    /// <summary>
    /// Видалити декількох учасників.
    /// </summary>
    Task<int> DeleteManyAsync(List<int> ids);

    /// <summary>
    /// Отримати список зустрічей,
    /// у яких бере участь користувач.
    /// </summary>
    Task<List<MeetingReadDTO>> GetMeetingsAsync(int participantId);
}