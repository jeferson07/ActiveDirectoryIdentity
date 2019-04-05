using System.Collections.Generic;
using System.Security.Principal;

namespace ADIdentity.AD.Model
{
    public class ActiveDirectoryUser
    {
        public SecurityIdentifier Sid { get; set; }
        public string DisplayName { get; set; }
        public string Email { get; set; }
        public bool Mapped { get; set; }
        public string Login { get; set; }
        public string UserName { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string Surname { get; set; }
        public string Initials { get; set; }
        public string VoiceTelephoneNumber { get; set; }
        public string JobTitle { get; set; }
        public string Department { get; set; }

        public List<ActiveDirectoryGroup> Groups { get; set; }
    }
}
