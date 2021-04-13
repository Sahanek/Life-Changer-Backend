using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Dtos
{
    public class PreferenceDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int  CategoryId { get; set; }
    }
}
