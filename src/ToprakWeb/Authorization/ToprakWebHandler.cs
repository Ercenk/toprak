namespace ToprakWeb.Authorization
{
    using System;
    using System.Collections.Generic;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Facebook;
    using Microsoft.AspNet.Authentication;
    using Microsoft.AspNet.Http.Authentication;
    using Microsoft.AspNet.Http.Features.Authentication;
    using Microsoft.Framework.Logging;

    internal class ToprakWebHandler<TOptions> : AuthenticationHandler<TOptions> where TOptions : ToprakWebOptions, new()
    {
        private readonly ILogger logger;
        private readonly Dictionary<string, Func<string, Task<ClaimsIdentity>>> authProviders;

        public ToprakWebHandler(ILogger logger)
        {
            this.logger = logger;
            this.authProviders = new Dictionary<string, Func<string, Task<ClaimsIdentity>>>()
            {
                {"none", this.NoAuth},
                {"facebook", this.FacebookAuth},
                {"google", this.GoogleAuth}
            };
        }

        private async Task<ClaimsIdentity> NoAuth(string token)
        {
            const string noEmail = "noemail";
            const string role = "User";
            var claims = new[]
                   {
                                new Claim("Email", noEmail), 
                                new Claim("Role", role)
                            };
            var identity = new ClaimsIdentity(claims, "None", noEmail, role);

            return await Task.FromResult(identity);
        }

        private async Task<ClaimsIdentity> GoogleAuth(string token)
        {
            throw new NotImplementedException();
        }

        private async Task<ClaimsIdentity> FacebookAuth(string token)
        {
            var facebookClient = new FacebookClient(token);

            dynamic me = await facebookClient.GetTaskAsync("me", new {fields = "first_name,last_name,email,id" });

            var role = Options.GetRole(me.email);
            var claims = new[]
                            {
                                new Claim("Email", me.email), new Claim("Id", me.id),
                                new Claim("Role", role), new Claim("FirstName", me.first_name),
                                new Claim("LastName", me.last_name)
                            };
            var identity = new ClaimsIdentity(claims, "Facebook", me.email, role);
            return identity;
        }

        protected override async Task<AuthenticationTicket> HandleAuthenticateAsync()
        {
            try
            {
                var token = string.Empty;
                var authType =  "none";

                if (Request.Headers.ContainsKey("ToprakAuth"))
                {
                    var headerValues = Request.Headers["ToprakAuth"].Split(';');
                    authType = headerValues[0];
                    token = headerValues[1];
                }
                var identity = await this.authProviders[authType](token);
                
                var properties = new AuthenticationProperties();

                return new AuthenticationTicket(new ClaimsPrincipal(identity), properties, ToprakWebOptions.AuthScheme);
            }
            catch (Exception exception)
            {
                var headers = Request.Headers;

                throw;
            }
        }

        protected override async Task HandleSignInAsync(SignInContext context)
        {
            await base.HandleSignInAsync(context);
        }
    }
}