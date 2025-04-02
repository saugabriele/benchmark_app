using AutoMapper;
using DBA;

namespace DTO
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<UserModel, UserDTO>();
            CreateMap<FileModel, FileDTO>();
        }
    }
}
