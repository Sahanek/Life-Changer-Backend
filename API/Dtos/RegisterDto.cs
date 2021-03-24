using Core.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace API.Dtos
{
    public class RegisterDto
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public Sex Sex { get; set; }

        [RegularExpression("^(\\(?\\+?[0 - 9] *\\)?)?[0-9_\\- \\(\\)]*$")]
        public string PhoneNumber { get; set; }

    }
}
