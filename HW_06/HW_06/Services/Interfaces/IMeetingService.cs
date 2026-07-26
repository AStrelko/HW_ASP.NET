using HW_06.DTOs.MeetingDTO;
using HW_06.Helpers.Pagination;
using HW_06.Helpers.QueryParameters;

namespace HW_06.Services.Interfaces;

/// <summary>
/// Сервіс для роботи із зустрічами.
/// </summary>
public interface IMeetingService
{
    /// <summary>
    /// Отримати список зустрічей.
    /// </summary>
    Task<PagedResult<MeetingReadDTO>> GetAllAsync(
        MeetingFilter filter,
        MeetingQueryParameters parameters);

    /// <summary>
    /// Отримати зустріч за ідентифікатором.
    /// </summary>
    Task<MeetingDetailDTO?> GetByIdAsync(int id);

    /// <summary>
    /// Створити нову зустріч.
    /// </summary>
    Task<MeetingReadDTO> CreateAsync(MeetingCreateDTO dto);

    /// <summary>
    /// Повністю оновити зустріч.
    /// </summary>
    Task<bool> UpdateAsync(int id, MeetingUpdateDTO dto);

    /// <summary>
    /// Частково оновити зустріч.
    /// </summary>
    Task<bool> PartialUpdateAsync(int id, MeetingPartialUpdateDTO dto);

    /// <summary>
    /// Видалити зустріч.
    /// </summary>
    Task<bool> DeleteAsync(int id);

    /// <summary>
    /// Видалити декілька зустрічей.
    /// </summary>
    Task<int> DeleteManyAsync(List<int> ids);

    /// <summary>
    /// Отримати всі зустрічі учасника.
    /// </summary>
    Task<List<MeetingReadDTO>> GetByParticipantAsync(int participantId);
}