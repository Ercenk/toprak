namespace ToprakWeb.Authorization
{
    using System;
    using Microsoft.AspNet.DataProtection;
    using Microsoft.AspNet.Authentication;
    using Microsoft.AspNet.Builder;
    using Microsoft.Framework.Logging;
    using Microsoft.Framework.OptionsModel;
    using Microsoft.Framework.WebEncoders;

    public class ToprakWebAuthMiddleware<TOptions> : AuthenticationMiddleware<TOptions> where TOptions : ToprakWebOptions, new()
    {
        protected override AuthenticationHandler<TOptions> CreateHandler()
        {
            return new ToprakWebHandler<TOptions>(this.Logger);
        }

        public ToprakWebAuthMiddleware(
            RequestDelegate next,
            IDataProtectionProvider dataProtectionProvider,
            ILoggerFactory loggerFactory,
            IUrlEncoder encoder,
            IOptions<TOptions> options,
            ConfigureOptions<TOptions> configureOptions) 
            : base(next, options, loggerFactory, encoder, configureOptions)
        {
            if (next == null)
            {
                throw new ArgumentNullException(nameof(next));
            }


            if (loggerFactory == null)
            {
                throw new ArgumentNullException(nameof(loggerFactory));
            }

            if (encoder == null)
            {
                throw new ArgumentNullException(nameof(encoder));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (string.IsNullOrEmpty(Options.FacebookAppId))
            {
                throw new ArgumentException($"No FacebookAppId {nameof(Options.FacebookAppId)}");
            ;
            }
            if (string.IsNullOrEmpty(Options.FacebookAppSecret))
            {
                throw new ArgumentException($"No FacebookAppSecret {nameof(Options.FacebookAppSecret)}");
            }

            if (string.IsNullOrEmpty(Options.GoogleClientId))
            {
                throw new ArgumentException($"No GoogleClientId {nameof(Options.GoogleClientId)}");
            }
            if (string.IsNullOrEmpty(Options.GoogleClientSecret))
            {
                throw new ArgumentException($"No GoogleClientSecret {nameof(Options.GoogleClientSecret)}");
            }
        }

        public ToprakWebAuthMiddleware(RequestDelegate next, IOptions<TOptions> options, ILoggerFactory loggerFactory, IUrlEncoder encoder, ConfigureOptions<TOptions> configureOptions)
            : base(next, options, loggerFactory, encoder, configureOptions)
        {
        }        
    }
}