using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Entities;

namespace API.Dtos
{
    public class UserPreferenceDto
    {
        public string AppUserId { get; set; }
        public PreferenceDto Preference { get; set; }
        public int Score { get; set; }
    }
}
