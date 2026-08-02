using AutoMapper;
using HW_06.DTOs.Files;
using HW_06.Models;

namespace HW_06.Profile;

/// <summary>
/// Містить правила перетворення
/// сутності приватного файлу
/// у DTO для клієнта.
/// </summary>
public class PrivateAttachmentMappingProfile : AutoMapper.Profile
{
    /// <summary>
    /// Ініціалізує профіль AutoMapper
    /// та налаштовує правила мапінгу
    /// приватних файлів.
    /// </summary>
    public PrivateAttachmentMappingProfile()
    {
        CreateMap<ParticipantPrivateFile, AttachmentPrivateDTO>()

            // Формує повне ім'я учасника-відправника.
            .ForCtorParam(
                nameof(AttachmentPrivateDTO.SenderFullName),
                options => options.MapFrom(source =>
                    $"{source.SenderParticipant.FirstName} " +
                    $"{source.SenderParticipant.LastName}".Trim()))

            // Формує повне ім'я учасника-отримувача.
            .ForCtorParam(
                nameof(AttachmentPrivateDTO.RecipientFullName),
                options => options.MapFrom(source =>
                    $"{source.RecipientParticipant.FirstName} " +
                    $"{source.RecipientParticipant.LastName}".Trim()))

            // Посилання на завантаження формується в сервісі.
            .ForCtorParam(
                nameof(AttachmentPrivateDTO.DownloadUrl),
                options => options.MapFrom(_ => string.Empty));
    }
}