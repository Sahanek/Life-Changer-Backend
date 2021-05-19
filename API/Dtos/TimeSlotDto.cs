using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Dtos
{
    public class TimeSlotDto
    {
        public DateTime StartOfFreeSlot { get; set; }
        public DateTime EndOfFreeSlot { get; set; }
        public TimeSpan Gap { get; set; }
    }

}
