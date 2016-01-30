namespace ToprakWeb.Authorization
{
    using System.Linq;
    using Microsoft.AspNet.Authentication;

    public class ToprakWebOptions: AuthenticationOptions
    {
        public ToprakWebOptions()
        {
            
        }

        public const string AuthScheme = "ToprakWeb";

        public string FacebookAppId { get; set; }
        public string FacebookAppSecret { get; set; }
        public string GoogleClientId { get; set; }
        public string GoogleClientSecret { get; set; }

        public string[] AdminIds { get; set; }
    

        public string GetRole(string id)
        {
            return this.AdminIds.Contains(id) ? "Admin" : "User";
        }
    }


}