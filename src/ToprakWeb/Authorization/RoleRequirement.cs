namespace ToprakWeb.Authorization
{
    using Microsoft.AspNet.Authorization;

    public class RoleRequirement : AuthorizationHandler<RoleRequirement>, IAuthorizationRequirement
    {
        private readonly string role;

        public RoleRequirement(string role)
        {
            this.role = role;
        }
        protected override void Handle(AuthorizationContext context, RoleRequirement requirement)
        {
            if (context.User.HasClaim("Role", this.role))
            {
                context.Succeed(requirement); 
                
            }
            else
            {
                context.Fail();
            }
        }
    }
}
