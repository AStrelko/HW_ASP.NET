using HW_06.DTOs.MeetingDTO;
using HW_06.DTOs.ParticipantDTO;
using HW_06.DTOs.Participants;
using HW_06.Helpers.Pagination;
using HW_06.Helpers.QueryParameters;
using Microsoft.AspNetCore.Http;

namespace HW_06.Services.Interfaces;

/// <summary>
/// Сервіс для роботи з учасниками.
/// Забезпечує отримання, оновлення,
/// видалення та пошук учасників,
/// а також роботу з їх аватарами.
/// </summary>
public interface IParticipantService
{
    /// <summary>
    /// Отримує список учасників із підтримкою
    /// пагінації, пошуку та сортування.
    /// </summary>
    /// <param name="parameters">
    /// Параметри пагінації, пошуку та сортування учасників.
    /// </param>
    /// <returns>
    /// Сторінку учасників разом із даними пагінації.
    /// </returns>
    Task<PagedResult<ParticipantReadDTO>> GetAllAsync(
        ParticipantQueryParameters parameters);

    /// <summary>
    /// Отримує учасника за його ідентифікатором.
    /// </summary>
    /// <param name="id">
    /// Унікальний ідентифікатор учасника.
    /// </param>
    /// <returns>
    /// Детальну інформацію про учасника або
    /// <see langword="null"/>, якщо учасника не знайдено.
    /// </returns>
    Task<ParticipantDetailDTO?> GetByIdAsync(int id);

    /// <summary>
    /// Повністю оновлює інформацію про учасника.
    /// </summary>
    /// <param name="id">
    /// Унікальний ідентифікатор учасника.
    /// </param>
    /// <param name="dto">
    /// Нові дані учасника.
    /// </param>
    /// <returns>
    /// <see langword="true"/>, якщо учасника успішно оновлено;
    /// інакше <see langword="false"/>.
    /// </returns>
    Task<bool> UpdateAsync(
        int id,
        ParticipantUpdateDTO dto);

    /// <summary>
    /// Частково оновлює інформацію про учасника.
    /// </summary>
    /// <param name="id">
    /// Унікальний ідентифікатор учасника.
    /// </param>
    /// <param name="dto">
    /// Дані полів, які необхідно оновити.
    /// </param>
    /// <returns>
    /// <see langword="true"/>, якщо учасника успішно оновлено;
    /// інакше <see langword="false"/>.
    /// </returns>
    Task<bool> PartialUpdateAsync(
        int id,
        ParticipantPartialUpdateDTO dto);

    /// <summary>
    /// Видаляє учасника.
    /// </summary>
    /// <param name="id">
    /// Унікальний ідентифікатор учасника.
    /// </param>
    /// <returns>
    /// <see langword="true"/>, якщо учасника успішно видалено;
    /// інакше <see langword="false"/>.
    /// </returns>
    Task<bool> DeleteAsync(int id);

    /// <summary>
    /// Видаляє декількох учасників за їх ідентифікаторами.
    /// </summary>
    /// <param name="ids">
    /// Список ідентифікаторів учасників, яких необхідно видалити.
    /// </param>
    /// <returns>
    /// Кількість фактично видалених учасників.
    /// </returns>
    Task<int> DeleteManyAsync(List<int> ids);

    /// <summary>
    /// Отримує список зустрічей, у яких бере участь учасник.
    /// </summary>
    /// <param name="participantId">
    /// Унікальний ідентифікатор учасника.
    /// </param>
    /// <returns>
    /// Список зустрічей учасника.
    /// Якщо учасник не має зустрічей, повертається порожній список.
    /// </returns>
    /// /// <exception cref="KeyNotFoundException">
    /// Виникає, якщо учасника із зазначеним
    /// ідентифікатором не знайдено.
    /// </exception>
    Task<List<MeetingReadDTO>> GetMeetingsAsync(int participantId);
    
    /// <summary>
    /// Отримати коротку інформацію про учасника
    /// разом із даними його аватара.
    /// </summary>
    /// <param name="participantId">
    /// Унікальний ідентифікатор учасника.
    /// </param>
    /// <returns>
    /// DTO учасника з даними аватара або
    /// <see langword="null"/>, якщо учасника не знайдено.
    /// </returns>
    Task<ParticipantAvatarDTO?> GetAvatarAsync(
        int participantId);

    /// <summary>
    /// Додати аватар учаснику.
    /// </summary>
    /// <param name="participantId">
    /// Унікальний ідентифікатор учасника.
    /// </param>
    /// <param name="file">
    /// Файл нового аватара.
    /// </param>
    /// <param name="cancellationToken">
    /// Токен скасування операції.
    /// </param>
    /// <returns>
    /// DTO учасника з даними доданого аватара або
    /// <see langword="null"/>, якщо учасника не знайдено.
    /// </returns>
    Task<ParticipantAvatarDTO?> UploadAvatarAsync(
        int participantId,
        IFormFile file,
        CancellationToken cancellationToken = default);
    
    
    /// <summary>
    /// Видаляє власний аватар учасника
    /// та повертає використання стандартного аватара.
    /// </summary>
    /// <param name="participantId">
    /// Унікальний ідентифікатор учасника.
    /// </param>
    /// <param name="cancellationToken">
    /// Токен скасування операції.
    /// </param>
    /// <returns>
    /// <see langword="true"/>, якщо учасника знайдено;
    /// інакше <see langword="false"/>.
    /// </returns>
    Task<bool> ResetAvatarAsync(
        int participantId,
        CancellationToken cancellationToken = default);
}