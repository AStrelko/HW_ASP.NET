
using AutoMapper;
using HW_06.DTOs.Meeting;
using HW_06.DTOs.Participant;
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
        //// Meeting -> DTO
        CreateMap<Meeting, MeetingreadDTO>()
            .ForMember(
                d => d.RoomNumber,
                o => o.MapFrom(s => s.Room != null
                    ? s.Room.NumberRoom
                    : (int?)null))
            .ForMember(
                d => d.ParticipantsCount,
                o => o.MapFrom(s => s.MeetingParticipants.Count));

        CreateMap<Meeting, MeetingditeylDTO>()
            .ForMember(
                d => d.RoomNumber,
                o => o.MapFrom(s => s.Room != null
                    ? s.Room.NumberRoom
                    : (int?)null))
            .ForMember(
                d => d.Participants,
                o => o.MapFrom(s =>
                    s.MeetingParticipants.Select(mp => mp.Participant)));

        //// Participant -> DTO
        CreateMap<Participant, ParticipantDTO>();
        
        //// DTO -> Meeting
        CreateMap<MeetingcreateDTO, Meeting>()
            .ForMember(
                d => d.MeetingParticipants,
                o => o.Ignore());

        CreateMap<MeetingupdateDTO, Meeting>()
            .ForMember(
                d => d.MeetingParticipants,
                o => o.Ignore());

        CreateMap<MeetingpartialUpdateDTO, Meeting>()
            .ForMember(
                d => d.MeetingParticipants,
                o => o.Ignore());
        
        //// Meeting -> DTO для редогування
        CreateMap<Meeting, MeetingupdateDTO>();
        
        CreateMap<Meeting, MeetingpartialUpdateDTO>();
        
        CreateMap<Participant, ParticipantReadDTO>();

        CreateMap<ParticipantCreateDTO, Participant>();

        CreateMap<ParticipantUpdateDTO, Participant>();

        CreateMap<Participant, ParticipantUpdateDTO>();
        
    }
}