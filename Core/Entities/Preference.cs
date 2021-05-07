using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities
{
    public class Preference
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int AverageTimeInMinutes { get; set; }
        public bool IsSpontaneus { get; set; }
        public string ImageUrl { get; set; }
        public int OffsetToPrepare { get; set; }
        public Category Category { get; set; }
        public int CategoryID { get; set; }
        public List<AppUserPreference> AppUsers { get; set; } = new();

    }
}
