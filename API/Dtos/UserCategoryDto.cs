using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Dtos
{
    public class UserCategoryDto
    {
        public string UserName { get; set; }
        public List<CategoryDto> Categories { get; set; }
    }
}
