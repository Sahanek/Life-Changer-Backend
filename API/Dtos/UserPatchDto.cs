using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace API.Dtos
{
    public class UserPatchDto
    {
        public string UserName { get; set; }
        public string Gender { get; set; }

        [RegularExpression("^(\\(?\\+?[0 - 9] *\\)?)?[0-9_\\- \\(\\)]*$")]
        public string PhoneNumber { get; set; }

        public string BirthDate { get; set; }
    }
}
