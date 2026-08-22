
using HW_06.DTOs.Files;
using HW_06.Services.Results;
using Microsoft.AspNetCore.Http;

namespace HW_06.Services.Interfaces;

/// <summary>
/// Визначає операції для роботи
/// з приватними файлами учасників.
/// </summary>
public interface IPrivateAttachmentService
{
    /// <summary>
    /// Завантажує приватний файл
    /// від одного учасника іншому.
    /// </summary>
    Task<AttachmentPrivateDTO?> UploadAsync(int senderParticipantId, int recipientParticipantId, IFormFile file);

    /// <summary>
    /// Повертає список приватних файлів,
    /// отриманих указаним учасником.
    /// </summary>
    Task<IReadOnlyCollection<AttachmentPrivateDTO>> GetReceivedFilesAsync(int participantId);

    /// <summary>
    /// Повертає список приватних файлів,
    /// надісланих указаним учасником.
    /// </summary>
    Task<IReadOnlyCollection<AttachmentPrivateDTO>> GetSentFilesAsync(int participantId);

    /// <summary>
    /// Повертає інформацію про приватний файл.
    /// </summary>
    Task<AttachmentPrivateDTO?> GetByIdAsync(int fileId, int participantId);

    /// <summary>
    /// Повертає приватний файл для завантаження.
    /// </summary>
    Task<AttachmentDownloadResult?> DownloadAsync(int fileId, int participantId);

    /// <summary>
    /// Видаляє приватний файл.
    /// </summary>
    Task<bool> DeleteAsync(int fileId, int participantId);
    
    /// <summary>
    /// Видаляє приватний файл
    /// без перевірки учасника.
    /// Використовується для адміністративних операцій.
    /// </summary>
    Task<bool> DeleteByAdminAsync(int fileId);
    
    /// <summary>
    /// Повертає список усіх приватних файлів
    /// усіх учасників.
    /// </summary>
    /// <returns>
    /// Колекція всіх приватних файлів.
    /// </returns>
    Task<IReadOnlyCollection<AttachmentPrivateDTO>> GetAllAsync();
    
}