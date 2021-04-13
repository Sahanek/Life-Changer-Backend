using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Entities;

namespace Core.Interfaces
{
    public interface IPreferenceService
    {
        IEnumerable<Preference> GetAll();
        public IEnumerable<Preference> GetPreferencesByCategory(List<int> Categories);
    }
}
