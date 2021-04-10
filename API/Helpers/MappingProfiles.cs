using API.Dtos;
using AutoMapper;
using Core.Entities;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Helpers
{
    public class MappingProfiles : Profile
    {
        public MappingProfiles()
        {
            CreateMap<JsonPatchDocument<UserInformationDto>, JsonPatchDocument<AppUser>>();
            CreateMap<Operation<UserInformationDto>, Operation<AppUser>>();
        }
    }
}
