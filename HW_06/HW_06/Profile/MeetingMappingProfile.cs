using AutoMapper;
using HW_06.DTOs.MeetingDTO;
using HW_06.DTOs.ParticipantDTO;
using HW_06.Models;

namespace HW_06.Profile;

/// <summary>
/// Містить конфігурацію AutoMapper для перетворення
/// сутностей зустрічей і учасників у відповідні DTO
/// та зворотного перетворення DTO у доменні моделі.
/// </summary>
/// <remarks>
/// Профіль налаштовує мапінг основних властивостей,
/// пов'язаних зустрічей, кімнат, учасників,
/// публічних вкладень і приватних файлів.
/// </remarks>
public class MeetingMappingProfile : AutoMapper.Profile
{
    /// <summary>
    /// Ініціалізує новий екземпляр профілю
    /// та реєструє правила мапінгу для сутностей
    /// <see cref="Meeting"/> і <see cref="Participant"/>.
    /// </summary>
    public MeetingMappingProfile()
    {
        // Meeting -> DTO
        
        CreateMap<Meeting, MeetingReadDTO>()
            // Перетворює ідентифікатор кімнати у її номер для відображення клієнту.
            .ForMember(
                destination => destination.RoomNumber,
                options => options.MapFrom(source =>
                    source.Room != null
                        ? source.Room.NumberRoom
                        : (int?)null))
            // Обчислює кількість учасників зустрічі за записами у проміжній таблиці.
            .ForMember(
                destination => destination.ParticipantsCount,
                options => options.MapFrom(source =>
                    source.MeetingParticipants.Count));
            
        
        CreateMap<Meeting, MeetingDetailDTO>()
            // Перетворює ідентифікатор кімнати у її номер для відображення.
            .ForMember(
                d => d.RoomNumber,
                o => o.MapFrom(s => s.Room != null
                    ? s.Room.NumberRoom
                    : (int?)null))
            // Формує повний список учасників зустрічі.
            .ForMember(
                d => d.Participants,
                o => o.MapFrom(s =>
                    s.MeetingParticipants
                        .Select(mp => mp.Participant)))
            // Додає список публічних документів зустрічі.
            .ForMember(
                destination => destination.Attachments,
                options => options.MapFrom(source => source.Attachments));

        CreateMap<Meeting, MeetingUpdateDTO>();

        CreateMap<Meeting, MeetingPartialUpdateDTO>();

        // DTO -> Meeting

        CreateMap<MeetingCreateDTO, Meeting>()
            // Список учасників створюється окремо в сервісі.
            .ForMember(
                d => d.MeetingParticipants,
                o => o.Ignore());

        CreateMap<MeetingUpdateDTO, Meeting>()
            // Список учасників оновлюється окремо в сервісі.
            .ForMember(
                d => d.MeetingParticipants,
                o => o.Ignore());

        CreateMap<MeetingPartialUpdateDTO, Meeting>()
            // Список учасників не змінюється під час часткового оновлення.
            .ForMember(
                d => d.MeetingParticipants,
                o => o.Ignore())
            // Оновлює лише ті властивості, які були передані в DTO.
                .ForAllMembers(options =>
                options.Condition(
                    (source, destination, sourceMember) =>
                        sourceMember != null));

        // Participant -> DTO

        CreateMap<Participant, ParticipantDTO>();

        CreateMap<Participant, ParticipantReadDTO>();

        CreateMap<Participant, ParticipantUpdateDTO>();

        CreateMap<Participant, ParticipantDetailDTO>()
            // Формує список зустрічей, у яких бере участь учасник.
            .ForMember(
                destination => destination.Meetings,
                options => options.MapFrom(source =>
                    source.MeetingParticipants
                        .Select(meetingParticipant =>
                            meetingParticipant.Meeting)))
            // Додає список приватних файлів, надісланих учасником.
            .ForMember(
                destination => destination.SentPrivateFiles,
                options => options.MapFrom(source =>
                    source.SentPrivateFiles))
            // Додає список приватних файлів, отриманих учасником.
            .ForMember(
                destination => destination.ReceivedPrivateFiles,
                options => options.MapFrom(source =>
                    source.ReceivedPrivateFiles));

        // DTO -> Participant

        CreateMap<ParticipantCreateDTO, Participant>()
            // Ідентифікатор створюється базою даних автоматично.
            .ForMember(
                destination => destination.ParticipantId,
                options => options.Ignore())
            // Ім’я файлу аватара задається окремо під час завантаження зображення.
            .ForMember(
                destination => destination.AvatarFileName,
                options => options.Ignore())
            // Зв’язки учасника із зустрічами створюються окремо в сервісі.
            .ForMember(
                destination => destination.MeetingParticipants,
                options => options.Ignore());
        
        CreateMap<ParticipantPartialUpdateDTO, Participant>()
            // Зв’язки учасника із зустрічами не змінюються під час часткового оновлення.
            .ForMember(
                destination => destination.MeetingParticipants,
                options => options.Ignore())
            // Оновлює лише ті властивості, які були передані в DTO.
            .ForAllMembers(options =>
                options.Condition(
                    (source, destination, sourceMember) =>
                        sourceMember != null));
    }
}