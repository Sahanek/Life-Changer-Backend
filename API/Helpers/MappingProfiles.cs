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
            CreateMap<JsonPatchDocument<UserPatchDto>, JsonPatchDocument<AppUser>>();
            CreateMap<Operation<UserPatchDto>, Operation<AppUser>>();
        }
    }
}
