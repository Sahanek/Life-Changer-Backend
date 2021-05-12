using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Dtos
{
    public class ActivityDto
    {
        public string Name { get; set; }
        public string DateStart { get; set; }
        public string TimeStart { get; set; }
        public string DateEnd { get; set; }
        public string TimeEnd { get; set; }
    }
}
