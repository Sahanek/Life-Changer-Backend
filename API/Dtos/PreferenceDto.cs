using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Dtos
{
    public class PreferenceDto
    {
      //  public int Id { get; set; }
        public string Name { get; set; }
        public int AverageTimeInMinutes { get; set; }
        public int OffsetToPrepare { get; set; }
        public bool IsSpontaneus { get; set; }
        public string ImageUrl { get; set; }
        public CategoryDto  Category { get; set; }
    }
}
