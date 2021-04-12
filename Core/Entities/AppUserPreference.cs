using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities
{
    public class AppUserPreference
    {
        public int Id { get; set; }
        public AppUser AppUser { get; set; }
        public string AppUserId { get; set; }
        public Preference Preference { get; set; }
        public int PreferenceId { get; set; }
        public int Quantity { get; set; }
        public int Score { get; set; }
    }
}
