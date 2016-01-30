using System.Security.Claims;

namespace ToprakWeb.ImageManager.Model
{
    using System.Collections.Generic;
    using System.Linq;

    public class User : IDbResult
    {
        public User(ClaimsPrincipal user, string ip)
        {
            var email = "noemail";
            if (user.Claims.Any(c => c.Type == "Email"))
            {
                email = user.Claims.First(c => c.Type == "Email").Value;
            }

            this.Email = email;
            var firstName = user.Claims.FirstOrDefault(c => c.Type == "FirstName");
            this.FirstName = firstName == null ? string.Empty : firstName.Value;
            var lastName = user.Claims.FirstOrDefault(c => c.Type == "LastName");
            this.LastName = lastName == null ? string.Empty : lastName.Value;
            this.Ip = ip;
        }

        public User()
        {
        }

        public string Email { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public int TotalResponses { get; set; }
        public string Ip { get; set; }
    }
}
