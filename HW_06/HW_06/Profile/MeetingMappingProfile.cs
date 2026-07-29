using AutoMapper;
using HW_06.DTOs.MeetingDTO;
using HW_06.DTOs.ParticipantDTO;
using HW_06.Models;

namespace HW_06.Profile;

/// <summary>
/// Профіль AutoMapper для мапінгу моделей Meeting та Participant
/// у відповідні DTO та навпаки.
/// </summary>
public class MeetingMappingProfile : AutoMapper.Profile
{
    public MeetingMappingProfile()
    {
        // Meeting -> DTO

        CreateMap<Meeting, MeetingReadDTO>()
            .ForMember(
                destination => destination.RoomNumber,
                options => options.MapFrom(source =>
                    source.Room != null
                        ? source.Room.NumberRoom
                        : (int?)null))
            .ForMember(
                destination => destination.ParticipantsCount,
                options => options.MapFrom(source =>
                    source.MeetingParticipants.Count));

        CreateMap<Meeting, MeetingDetailDTO>()
            .ForMember(
                d => d.RoomNumber,
                o => o.MapFrom(s => s.Room != null
                    ? s.Room.NumberRoom
                    : (int?)null))
            .ForMember(
                d => d.Participants,
                o => o.MapFrom(s =>
                    s.MeetingParticipants
                        .Select(mp => mp.Participant)));

        CreateMap<Meeting, MeetingUpdateDTO>();

        CreateMap<Meeting, MeetingPartialUpdateDTO>();

        // DTO -> Meeting

        CreateMap<MeetingCreateDTO, Meeting>()
            .ForMember(
                d => d.MeetingParticipants,
                o => o.Ignore());

        CreateMap<MeetingUpdateDTO, Meeting>()
            .ForMember(
                d => d.MeetingParticipants,
                o => o.Ignore());

        CreateMap<MeetingPartialUpdateDTO, Meeting>()
            .ForMember(
                d => d.MeetingParticipants,
                o => o.Ignore())
            .ForAllMembers(options =>
                options.Condition(
                    (source, destination, sourceMember) =>
                        sourceMember != null));

        // Participant -> DTO

        CreateMap<Participant, ParticipantDTO>();

        CreateMap<Participant, ParticipantReadDTO>();

        CreateMap<Participant, ParticipantUpdateDTO>();

        CreateMap<Participant, ParticipantDetailDTO>()
            .ForMember(
                destination => destination.Meetings,
                options => options.MapFrom(source =>
                    source.MeetingParticipants
                        .Select(meetingParticipant =>
                            meetingParticipant.Meeting)));

        // DTO -> Participant

        CreateMap<ParticipantCreateDTO, Participant>()
            .ForMember(
                destination => destination.ParticipantId,
                options => options.Ignore())
            .ForMember(
                destination => destination.AvatarFileName,
                options => options.Ignore())
            .ForMember(
                destination => destination.MeetingParticipants,
                options => options.Ignore());

        CreateMap<ParticipantPartialUpdateDTO, Participant>()
            .ForMember(
                destination => destination.MeetingParticipants,
                options => options.Ignore())
            .ForAllMembers(options =>
                options.Condition(
                    (source, destination, sourceMember) =>
                        sourceMember != null));
    }
}