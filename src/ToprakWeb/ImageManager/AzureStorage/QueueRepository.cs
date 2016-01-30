namespace ToprakWeb.ImageManager.AzureStorage
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Framework.Logging;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Queue;

    public class QueueRepository : IQueueRepository
    {
        private readonly ILogger logger;

        private readonly IDictionary<string, CloudQueue> queues;

        public QueueRepository(IStorageConnectionFactory connectionFactory, ILogger logger)
        {
            this.logger = logger;

            CloudStorageAccount account;
            var queueNames =
                typeof (QueueNames).GetFields(BindingFlags.Static | BindingFlags.Public)
                    .Where(f => f.IsLiteral)
                    .Select(f => (string) f.GetValue(null));

            this.queues = new Dictionary<string, CloudQueue>();

            if (connectionFactory.TryGetAccount(out account))
            {
                var cloudQueueClient = account.CreateCloudQueueClient();
                foreach (var queueName in queueNames)
                {
                    var queue = cloudQueueClient.GetQueueReference(queueName);
                    queue.CreateIfNotExists();
                    this.queues.Add(queueName, queue);
                }
            }
            else
            {
                this.logger.LogError("Cannot contruct queue repository, invalid connection.");
            }           
        }

        public async Task AddMessageAsync(string queueName, CloudQueueMessage message, CancellationToken cancellationToken)
        {
            var queue = this.queues[queueName];
            await queue.AddMessageAsync(message, cancellationToken);
        }

        public async Task<CloudQueueMessage> GetMessageAsync(string queueName, CancellationToken cancellationToken)
        {
            var queue = this.queues[queueName];
            var message = await queue.GetMessageAsync(TimeSpan.FromMinutes(5), null, null, cancellationToken);

            return message;
        }

        public async Task DeleteMessageAsync(string queueName, string messageId, string popReceipt, CancellationToken cancellationToken)
        {
            var queue = this.queues[queueName];
            await queue.DeleteMessageAsync(messageId, popReceipt, cancellationToken);
        }

        public async Task<int?> GetRemainingTutanakQueueLength(CancellationToken cancellationToken)
        {
            await this.queues[QueueNames.ToBeProcessed].FetchAttributesAsync(cancellationToken);
            return this.queues[QueueNames.ToBeProcessed].ApproximateMessageCount;
        }
    }
}