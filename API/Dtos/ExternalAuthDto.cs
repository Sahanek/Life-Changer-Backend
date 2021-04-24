using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace API.Dtos
{
    public class ExternalAuthDto
    {
        public string Provider { get; set; }
        [Required]
        public string Token { get; set; }
    }
}
