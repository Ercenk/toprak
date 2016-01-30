namespace ToprakProcessing
{
    using System;
    using System.Data.SqlClient;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using ImageManager.Model;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Framework.Logging;
    using Microsoft.Framework.Logging.EventLog;
    using Microsoft.WindowsAzure;
    using Microsoft.WindowsAzure.Storage.Queue;

    using ToprakWeb.ImageManager;
    using ToprakWeb.ImageManager.AzureStorage;
    using ToprakWeb.ImageManager.Model;
    using ToprakWeb.ResultsRepository;

    public class Functions
    {
        // This function will get triggered/executed when a new message is written 
        // on an Azure Queue called queue.
        public async static Task NewImagesTrigger([QueueTrigger(QueueNames.NewImages)]CloudQueueMessage message, CancellationToken cancellationToken)
        {
            var connectionFactory = new StorageConnectionFactory(CloudConfigurationManager.GetSetting("storageAccountConnection"));
            var factory = new LoggerFactory();
            factory.AddConsole();
            factory.AddEventLog();

            var logger = factory.CreateLogger("ToprakWebjobs");

            var imageManager = new ImageManagerService(connectionFactory, logger);

            await imageManager.AddToCirculation(true, cancellationToken);

        }

        public async static Task ProcessedImagesTrigger([QueueTrigger(QueueNames.Seen)]CloudQueueMessage message, CancellationToken cancellationToken)
        {
            var connectionFactory = new StorageConnectionFactory(CloudConfigurationManager.GetSetting("storageAccountConnection"));
            var factory = new LoggerFactory();
            factory.AddConsole();
            factory.AddEventLog();

            var logger = factory.CreateLogger("ToprakWebjobs");
            var imageRepository = new ImageRepository(connectionFactory, logger);
            var queueRepository = new QueueRepository(connectionFactory, logger);
            IResultsRepository resultsRepository = new ResultsRepository(CloudConfigurationManager.GetSetting("Database"), logger);

            var envelope = TutanakDataEnvelope.GorulmusTutanakMesajiFactory(message.AsString);
            envelope.TutanakData.Image = (new Uri(envelope.TutanakData.Image)).Segments[2];

            await resultsRepository.RecordResult(envelope);

            var imageSeen = envelope.TutanakData;

            var results = (await resultsRepository.GetTutanakResultsAsync(imageSeen.Image)).ToList();

            var compareProperties =
                typeof(GorulmusTutanakMesaji).GetProperties()
                    .Where(p => p.GetCustomAttributes(false).All(a => a.GetType() != typeof(NoCompareAttribute)))
                    .OrderBy(p => p.Name)
                    .Select(p => p.Name);

            var resultHashGroups =
                results.Select(
                    r =>
                    compareProperties.Select(p => r.GetType().GetProperty(p).GetValue(r)).Aggregate((i, j) => $"{i},{j}"))
                    .GroupBy(h => h)
                    .Select(g => g.Count())
                    .OrderByDescending(c => c).ToList();

            var sameReadLimit = Convert.ToInt32(CloudConfigurationManager.GetSetting("SameReadLimit"));

            if (resultHashGroups.Any() && resultHashGroups.Max() >= sameReadLimit)
            {
                await imageRepository.TransferToProcessed(imageSeen.Image, cancellationToken);
                await resultsRepository.RecordReadSuccessResult(envelope);
            } else 
            {
                await
                    queueRepository.AddMessageAsync(
                        QueueNames.ToBeProcessed,
                        new CloudQueueMessage(imageRepository.GetUnprocessedUri(imageSeen.Image).AbsoluteUri),
                        cancellationToken);
            }
        }
    }
}
