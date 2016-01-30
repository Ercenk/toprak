namespace ToprakWeb
{
    using System;
    using Authorization;
    using Autofac;
    using Autofac.Framework.DependencyInjection;

    using Microsoft.ApplicationInsights;
    using Microsoft.AspNet.Builder;
    using Microsoft.AspNet.Hosting;
    using Microsoft.AspNet.StaticFiles;
    using Microsoft.Dnx.Runtime;
    using Microsoft.Framework.Configuration;
    using Microsoft.Framework.DependencyInjection;
    using Microsoft.Framework.Logging;
    using Microsoft.AspNet.Diagnostics;
    using Microsoft.AspNet.Http;
    using Microsoft.Framework.WebEncoders;

    public class Startup
    {
        private readonly IConfiguration config;

        public Startup(IHostingEnvironment env, IApplicationEnvironment appEnvironment)
        {
            var configBuilder = new ConfigurationBuilder(appEnvironment.ApplicationBasePath);
            configBuilder.AddJsonFile("config.json");
            configBuilder.AddEnvironmentVariables();
            if (env.IsEnvironment("Development"))
            {
                configBuilder.AddApplicationInsightsSettings(developerMode: true);
            }
            this.config = configBuilder.Build();
        }

        // This method gets called by a runtime.C:\Projects\toprak\ToprakWeb\gruntfile.js
        // Use this method to add services to the container
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddApplicationInsightsTelemetry(this.config);
            services.AddAuthentication(options => options.SignInScheme = "ToprakWeb");
            services.ConfigureAuthorization(
                options =>
                    {
                        options.AddPolicy("MustBeAdmin", policy => policy.Requirements.Add(new RoleRequirement("Admin")));
                        options.AddPolicy("MustBeUser", policy => policy.Requirements.Add(new RoleRequirement("User")));
                    });
            services.AddMvc();          

            var builder = new ContainerBuilder();
            builder.RegisterModule(new AutofacModule(config));
            builder.Populate(services);
            var container = builder.Build();

            return container.Resolve<IServiceProvider>();
        }

        // Configure is called after ConfigureServices is called.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.MinimumLevel = LogLevel.Information;
            loggerFactory.AddConsole();
            loggerFactory.AddDebug();

            loggerFactory.AddEventLog();
            var logger = loggerFactory.CreateLogger("ToprakWeb");

            if (env.IsDevelopment())
            {
                app.UseBrowserLink();
                app.UseErrorPage();
            }

            app.UseApplicationInsightsRequestTelemetry();
            app.UseErrorHandler(
                errorApp =>
                    {
                        var telemetryClient = new TelemetryClient();
                        errorApp.Run(
                            async context =>
                                {
                                    context.Response.StatusCode = 200;
                                    context.Response.ContentType = "text/html";
                                    await context.Response.WriteAsync("<html><body>\r\n");
                                    await
                                        context.Response.WriteAsync(
                                            "Beklenmiyen bir hata olustu.<br>\r\n");

                                    var error = context.GetFeature<IErrorHandlerFeature>();
                                    if (error != null)
                                    {
                                        // This error would not normally be exposed to the client
                                        logger.LogCritical($"{error.Error.Message} \n {error.Error.StackTrace}");
                                    }
                                    await context.Response.WriteAsync("<br><a href=\"/\">Home</a><br>\r\n");
                                    await context.Response.WriteAsync("</body></html>\r\n");
                                    await context.Response.WriteAsync(new string(' ', 512)); // Padding for IE
                                });
                    });
            app.UseApplicationInsightsExceptionTelemetry();

            var adminEmails = this.config["adminEmails"];
            var facebookAppId = this.config["FacebookAppId"];
            if (string.IsNullOrEmpty(facebookAppId) && env.IsDevelopment())
            {
                facebookAppId = this.config["AppSettings:FacebookAppId"];
            }
            var facebookAppSecret = this.config["FacebookAppSecret"];
            if (string.IsNullOrEmpty(facebookAppSecret) && env.IsDevelopment())
            {
                facebookAppSecret = this.config["AppSettings:FacebookAppSecret"];
            }
            app.UseToprakWebAuthentication(
                "ToprakWeb",
                (options) =>
                {
                    options.FacebookAppId = facebookAppId;
                        options.FacebookAppSecret = facebookAppSecret;
                        options.GoogleClientId = this.config["AppSettings:GoogleClientId"];
                        options.GoogleClientSecret = this.config["AppSettings:GoogleClientSecret"];
                        options.AdminIds = adminEmails.Split(',');
                    });
            app.UseDefaultFiles();

            var staticFileOptions = new StaticFileOptions();
            var typeProvider = new FileExtensionContentTypeProvider();
            if (!typeProvider.Mappings.ContainsKey(".woff2"))
            {
                typeProvider.Mappings.Add(".woff2", "application/font-woff2");
            }
            staticFileOptions.ContentTypeProvider = typeProvider;

            app.UseStaticFiles(staticFileOptions);
            app.UseMvc();
            app.UseApplicationInsightsExceptionTelemetry();
        }
    }
}
