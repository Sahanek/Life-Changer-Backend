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
        Task<IEnumerable<Preference>> GetAll();
        Task<IEnumerable<Preference>> GetPreferencesByCategory(List<int> Categories);
        Task<bool> UpdateUserCategories(List<int> Categories, AppUser user);
        Task<AppUser> GetUserWithNestedEntities(AppUser user);
        Task<IEnumerable<AppUserPreference>> GetAppUserPreferenceOfUser(AppUser user, bool WithNested);
    }
}
