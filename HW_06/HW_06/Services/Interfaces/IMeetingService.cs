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
    /// Отримує список зустрічей із підтримкою
    /// фільтрації, пагінації та сортування.
    /// </summary>
    /// <param name="filter">
    /// Параметри фільтрації зустрічей.
    /// </param>
    /// <param name="parameters">
    /// Параметри пагінації, пошуку та сортування.
    /// </param>
    /// <returns>
    /// Сторінку зустрічей разом із даними пагінації.
    /// </returns>
    Task<PagedResult<MeetingReadDTO>> GetAllAsync(
        MeetingFilter filter,
        MeetingQueryParameters parameters);

    /// <summary>
    /// Отримує зустріч за її ідентифікатором.
    /// </summary>
    /// <param name="id">
    /// Унікальний ідентифікатор зустрічі.
    /// </param>
    /// <returns>
    /// Детальну інформацію про зустріч або
    /// <see langword="null"/>, якщо зустріч не знайдено.
    /// </returns>
    Task<MeetingDetailDTO?> GetByIdAsync(int id);

    /// <summary>
    /// Створює нову зустріч.
    /// </summary>
    /// <param name="dto">
    /// Дані для створення зустрічі.
    /// </param>
    /// <returns>
    /// Створену зустріч.
    /// </returns>
    Task<MeetingReadDTO> CreateAsync(MeetingCreateDTO dto);

    /// <summary>
    /// Повністю оновлює інформацію про зустріч.
    /// </summary>
    /// <param name="id">
    /// Унікальний ідентифікатор зустрічі.
    /// </param>
    /// <param name="dto">
    /// Нові дані зустрічі.
    /// </param>
    /// <returns>
    /// <see langword="true"/>, якщо зустріч успішно оновлено;
    /// інакше <see langword="false"/>.
    /// </returns>
    Task<bool> UpdateAsync(int id, MeetingUpdateDTO dto);

    /// <summary>
    /// Частково оновлює інформацію про зустріч.
    /// </summary>
    /// <param name="id">
    /// Унікальний ідентифікатор зустрічі.
    /// </param>
    /// <param name="dto">
    /// Дані полів, які необхідно оновити.
    /// </param>
    /// <returns>
    /// <see langword="true"/>, якщо зустріч успішно оновлено;
    /// інакше <see langword="false"/>.
    /// </returns>
    Task<bool> PartialUpdateAsync(int id, MeetingPartialUpdateDTO dto);

    /// <summary>
    /// Видаляє зустріч.
    /// </summary>
    /// <param name="id">
    /// Унікальний ідентифікатор зустрічі.
    /// </param>
    /// <returns>
    /// <see langword="true"/>, якщо зустріч успішно видалено;
    /// інакше <see langword="false"/>.
    /// </returns>
    Task<bool> DeleteAsync(int id);

    /// <summary>
    /// Видаляє декілька зустрічей за їх ідентифікаторами.
    /// </summary>
    /// <param name="ids">
    /// Список ідентифікаторів зустрічей, які необхідно видалити.
    /// </param>
    /// <returns>
    /// Кількість фактично видалених зустрічей.
    /// </returns>
    Task<int> DeleteManyAsync(List<int> ids);

    /// <summary>
    /// Отримує список зустрічей, у яких бере участь вказаний учасник.
    /// </summary>
    /// <param name="participantId">
    /// Унікальний ідентифікатор учасника.
    /// </param>
    /// <returns>
    /// Список зустрічей учасника.
    /// Якщо учасник не бере участі в жодній зустрічі,
    /// повертається порожній список.
    /// </returns>
    Task<List<MeetingReadDTO>> GetByParticipantAsync(int participantId);
}