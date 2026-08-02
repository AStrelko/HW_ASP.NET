using AutoMapper;
using HW_06.DTOs.Files;
using HW_06.Models;

namespace HW_06.Profile;

/// <summary>
/// Містить правила перетворення
/// публічних вкладень зустрічей
/// у DTO для клієнта.
/// </summary>
public class AttachmentMappingProfile : AutoMapper.Profile
{
    /// <summary>
    /// Ініціалізує профіль AutoMapper
    /// та налаштовує правила мапінгу
    /// публічних вкладень зустрічей.
    /// </summary>
    public AttachmentMappingProfile()
    {
        CreateMap<MeetingAttachment, AttachmentPublicDTO>()

            // Посилання на завантаження формується окремо в сервісі.
            .ForCtorParam(
                nameof(AttachmentPublicDTO.DownloadUrl),
                options => options.MapFrom(_ => string.Empty));
    }
}