namespace ToprakWeb
{
    using Autofac;
    using ImageManager;
    using ImageManager.AzureStorage;

    using Microsoft.Framework.Configuration;
    using Microsoft.Framework.Logging;

    using ToprakWeb.ResultsRepository;

    public class AutofacModule : Module
    {
        private readonly IConfiguration config;

        public AutofacModule(IConfiguration config)
        {
            this.config = config;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.Register(
                c =>
                    {
                        var factory = new LoggerFactory();
                        factory.AddEventLog();
                        return factory.CreateLogger("ToprakWeb");
                    }).As<ILogger>().InstancePerLifetimeScope();

            var connectionString = this.config["Data:ImageRepository:ConnectionString"];
            var databaseConnectionString = this.config["Data:Database:ConnectionString"];

            builder.Register(
                c =>
                connectionString == "Dev"
                    ? new StorageConnectionFactory()
                    : new StorageConnectionFactory(connectionString))
                .As<IStorageConnectionFactory>()
                .InstancePerLifetimeScope();

            builder.Register(c => new QueueRepository(c.Resolve<IStorageConnectionFactory>(), c.Resolve<ILogger>()))
                .As<IQueueRepository>()
                .InstancePerLifetimeScope();

            builder.Register(c => new ImageRepository(c.Resolve<IStorageConnectionFactory>(), c.Resolve<ILogger>()))
                .As<IImageRepository>()
                .InstancePerLifetimeScope();


            builder.Register(c => new ImageManagerService(c.Resolve<IQueueRepository>(), c.Resolve<IImageRepository>(), c.Resolve<ILogger>()))
                .As<IImageManagerService>()
                .InstancePerLifetimeScope();

            builder.Register(c => new ResultsRepository.ResultsRepository(databaseConnectionString, c.Resolve<ILogger>()))
                .As<IResultsRepository>()
                .InstancePerLifetimeScope();
        }
    }
}
