namespace ToprakWeb.Authorization
{
    using System;
    using Microsoft.AspNet.Builder;
    using Microsoft.Framework.Logging;
    using Microsoft.Framework.OptionsModel;

    public static class ToprakWebAppExtensions
    {
        public static IApplicationBuilder UseToprakWebAuthentication(
            this IApplicationBuilder app,
            string authenticationScheme,
            Action<ToprakWebOptions> configureOptions)
        {
            return
                app.UseMiddleware<ToprakWebAuthMiddleware<ToprakWebOptions>>(
                    new ConfigureOptions<ToprakWebOptions>(
                        options =>
                            {
                                options.AuthenticationScheme = authenticationScheme;
                                configureOptions?.Invoke(options);
                            }));
        }
    }
}
