using AutoMapper;
using SophieHR.Api.Models;
using SophieHR.Api.Models.DTOs.Notes;

namespace SophieHR.Api.Profiles
{
    public class NoteProfile : Profile
    {
        public NoteProfile()
        {
            CreateMap<Note, NoteCreateDto>().ReverseMap();
            CreateMap<Note, NoteDetailDto>().ReverseMap();
        }
    }
}