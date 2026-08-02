using HW_06.DTOs.Files;
using HW_06.Models;
using HW_06.Services.Results;
using Microsoft.AspNetCore.Http;

namespace HW_06.Services.Interfaces;

/// <summary>
/// Определяет операции для работы с публичными файлами-вложениями встреч.
/// </summary>
public interface IAttachmentService
{
    /// <summary>
    /// Загружает файл и прикрепляет его к указанной встрече.
    /// </summary>
    /// <param name="meetingId">Идентификатор встречи.</param>
    /// <param name="file">Загружаемый файл.</param>
    /// <returns>
    /// Данные созданного файла-вложения либо <see langword="null"/>,
    /// если встреча не найдена.
    /// </returns>
    Task<AttachmentPublicDTO?> UploadAsync(
        int meetingId,
        IFormFile file);

    /// <summary>
    /// Возвращает список публичных файлов, прикреплённых к встрече.
    /// </summary>
    /// <param name="meetingId">Идентификатор встречи.</param>
    /// <returns>Список файлов-вложений встречи.</returns>
    Task<IReadOnlyCollection<AttachmentPublicDTO>?> GetAllAsync(
        int meetingId);

    /// <summary>
    /// Возвращает данные файла-вложения по его идентификатору.
    /// </summary>
    /// <param name="meetingId">Идентификатор встречи.</param>
    /// <param name="attachmentId">Идентификатор файла-вложения.</param>
    /// <returns>
    /// Данные файла-вложения либо <see langword="null"/>,
    /// если файл не найден или не относится к указанной встрече.
    /// </returns>
    Task<AttachmentDownloadResult?> DownloadAsync(
        int meetingId,
        int attachmentId);
    /// <summary>
    /// Удаляет файл-вложение из хранилища и базы данных.
    /// </summary>
    /// <param name="meetingId">Идентификатор встречи.</param>
    /// <param name="attachmentId">Идентификатор файла-вложения.</param>
    /// <returns>
    /// <see langword="true"/>, если файл удалён;
    /// иначе <see langword="false"/>.
    /// </returns>
    Task<bool> DeleteAsync(
        int meetingId,
        int attachmentId);
}