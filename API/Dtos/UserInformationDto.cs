using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Dtos
{
    public class UserInformationDto
    {
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Gender { get; set; }
        public string PhoneNumber { get; set; }

        public string BirthDate { get; set; }
    }
}
