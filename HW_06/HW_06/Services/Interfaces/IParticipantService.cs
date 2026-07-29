using HW_06.DTOs.MeetingDTO;
using HW_06.DTOs.ParticipantDTO;
using HW_06.DTOs.Participants;
using HW_06.Helpers.Pagination;
using HW_06.Helpers.QueryParameters;
using Microsoft.AspNetCore.Http;

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
    /// Створює нового учасника та додає аватар,
    /// якщо файл був переданий.
    /// </summary>
    /// <param name="dto">
    /// Дані нового учасника та необов’язковий аватар.
    /// </param>
    /// <param name="cancellationToken">
    /// Токен скасування операції.
    /// </param>
    /// <returns>
    /// Створений учасник.
    /// </returns>
    Task<ParticipantReadDTO> CreateAsync(
        ParticipantCreateDTO dto,
        CancellationToken cancellationToken = default);

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

/*
    /// <summary>
    /// Видалити аватар учасника.
    /// </summary>
    /// <param name="participantId">
    /// Унікальний ідентифікатор учасника.
    /// </param>
    /// <param name="cancellationToken">
    /// Токен скасування операції.
    /// </param>
    /// <returns>
    /// <see langword="true"/>, якщо учасника знайдено
    /// та операцію виконано; інакше <see langword="false"/>.
    /// </returns>
    Task<bool> DeleteAvatarAsync(
        int participantId,
        CancellationToken cancellationToken = default);
    
    */
}