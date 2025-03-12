using Microsoft.AspNetCore.Identity;

namespace PersonalDictionaryProject.Models
{
    public class User : IdentityUser
    {
        public string FullName { get; set; }
    }
}
