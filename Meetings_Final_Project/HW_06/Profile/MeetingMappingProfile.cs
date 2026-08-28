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
            // Перетворює номер кімнати
            // у значення для відображення клієнту.
            .ForMember(
                destination => destination.RoomNumber,
                options => options.MapFrom(source =>
                    source.Room != null
                        ? source.Room.NumberRoom
                        : (int?)null))
            // Обчислює кількість учасників зустрічі.
            .ForMember(
                destination => destination.ParticipantsCount,
                options => options.MapFrom(source =>
                    source.MeetingParticipants.Count));

        CreateMap<Meeting, MeetingDetailDTO>()
            // Перетворює номер кімнати
            // у значення для відображення клієнту.
            .ForMember(
                destination => destination.RoomNumber,
                options => options.MapFrom(source =>
                    source.Room != null
                        ? source.Room.NumberRoom
                        : (int?)null))
            // Формує список учасників зустрічі.
            .ForMember(
                destination => destination.Participants,
                options => options.MapFrom(source =>
                    source.MeetingParticipants
                        .Select(meetingParticipant =>
                            meetingParticipant.Participant)))
            // Додає список публічних вкладень зустрічі.
            .ForMember(
                destination => destination.Attachments,
                options => options.MapFrom(source =>
                    source.Attachments));


        // DTO -> Meeting

        CreateMap<MeetingCreateDTO, Meeting>()
            // Зв'язки з учасниками
            // створюються окремо в сервісі.
            .ForMember(
                destination => destination.MeetingParticipants,
                options => options.Ignore());

        // Participant -> DTO

        CreateMap<Participant, ParticipantDTO>()
            .ForMember(
                destination => destination.Email,
                options => options.MapFrom(source =>
                    source.ApplicationUser != null
                        ? source.ApplicationUser.Email
                        : null))
            .ForMember(
                destination => destination.Position,
                options => options.MapFrom(source =>
                    source.Position));

        CreateMap<Participant, ParticipantReadDTO>()
            .ForMember(
                destination => destination.Email,
                options => options.MapFrom(source =>
                    source.ApplicationUser != null
                        ? source.ApplicationUser.Email
                        : null));

        CreateMap<Participant, ParticipantDetailDTO>()
            // Email зберігається в ApplicationUser, а не в Participant.
            .ForMember(
                destination => destination.Email,
                options => options.MapFrom(source =>
                    source.ApplicationUser != null
                        ? source.ApplicationUser.Email
                        : null))
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
    }
}
