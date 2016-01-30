namespace ToprakProcessing
{
    using System;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Framework.Logging;
    using Microsoft.WindowsAzure;
    using ToprakWeb.ImageManager.AzureStorage;

    // To learn more about Microsoft Azure WebJobs SDK, please see http://go.microsoft.com/fwlink/?LinkID=320976
    public class Program
    {
        // Please set the following connection strings in app.config for this WebJob to run:
        // AzureWebJobsDashboard and AzureWebJobsStorage

        public static void Main()
        {
            var connectionFactory = new StorageConnectionFactory(CloudConfigurationManager.GetSetting("storageAccountConnection"));
            var factory = new LoggerFactory();
            factory.AddConsole();
            factory.AddEventLog();
            var logger = factory.CreateLogger("ToprakWebjobs");

            AppDomain.CurrentDomain.UnhandledException +=
                (sender, args) => { logger.LogCritical(args.ExceptionObject.ToString()); };
        
            var host = new JobHost();
            // The following code ensures that the WebJob will be running continuously
            host.RunAndBlock();
        }
    }
}
