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
        [Required]
        public string UserName { get; set; }
        [Required]
        [RegularExpression("(?=^.{8,15}$)(?=.*\\d)(?=.*[a-z])(?=.*[A-Z])(?=.*[!@#$%^&amp;*()_+}{&quot;:;'?/&gt;.&lt;,])(?!.*\\s).*$",
            ErrorMessage = "Password must have 1 Uppercase, 1 Lowercase, 1 number, 1 non alphanumeric and 8 to 15 characters")]
        public string Password { get; set; }
        [Required]
        public string Email { get; set; }
        public string Gender { get; set; }

        [RegularExpression("^(\\(?\\+?[0 - 9] *\\)?)?[0-9_\\- \\(\\)]*$")]
        public string PhoneNumber { get; set; }

        public string BirthDate { get; set; }
    }
}
