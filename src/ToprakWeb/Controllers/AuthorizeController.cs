namespace ToprakWeb.Controllers
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using Microsoft.AspNet.Authorization;
    using Microsoft.AspNet.Mvc;

    using ToprakWeb.Authorization;

    [Route("api/v1/[controller]")]
    [Authorize(ActiveAuthenticationSchemes = ToprakWebOptions.AuthScheme)]
    public class AuthorizeController : Controller
    {
        [HttpGet]
        [ResponseCache(NoStore = true)]
        public AuthorizeResult GetRole(string token)
        {
            return new AuthorizeResult() { Value = Request.HttpContext.User.Claims.FirstOrDefault(c => c.Type == "Role").Value};
        }     
    }

    public class AuthorizeResult
    {
        public string Value { get; set; }
    }
}
