
using AutoMapper;
using HW_06.DTOs.Meeting;
using HW_06.Models;

namespace HW_06.Profile;

public class MeetingMappingProfile: AutoMapper.Profile
{
    public MeetingMappingProfile()
    {
        CreateMap<Meeting, MeetingreadDTO>();
        CreateMap<Meeting, MeetingditeylDTO>();
        CreateMap<Meeting, MeetingupdateDTO>();
        CreateMap<Meeting, MeetingpartialUpdateDTO>();
        CreateMap<MeetingcreateDTO, Meeting>();
        CreateMap<MeetingupdateDTO, Meeting>();
        
    }
}