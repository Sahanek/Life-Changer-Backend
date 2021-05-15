using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace API.Dtos
{
    public class ActivityDto
    {
        public string Name { get; set; }
        [Required]
        public string DateStart { get; set; }
        [Required]
        public string TimeStart { get; set; }
        [Required]
        public string DateEnd { get; set; }
        [Required]
        public string TimeEnd { get; set; }
    }
}
