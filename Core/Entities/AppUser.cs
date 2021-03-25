using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities
{
    //If you want to know more check the implemantation of IdentityUser(Shortcut F12)
    public class AppUser : IdentityUser
    {
        public Gender? Gender { get; set; }

        //+ some related entites with user e.g. Agenda

    }

    public enum Gender
    {
        Male,
        Female
    }
}
