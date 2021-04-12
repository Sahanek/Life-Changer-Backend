using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Dtos
{
    public class UserPreferenceDto
    {
        public string Username { get; set; }
        public string Preference { get; set; }
        public int Score { get; set; }
    }
}
