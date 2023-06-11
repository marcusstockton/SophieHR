using AutoMapper;
using SophieHR.Api.Models;
using SophieHR.Api.Models.DTOs.LeaveRequest;

namespace SophieHR.Api.Profiles
{
    public class LeaveRequestProfile : Profile
    {
        public LeaveRequestProfile()
        {
            CreateMap<CreateLeaveRequest, LeaveRequest>().ReverseMap();
        }
    }
}