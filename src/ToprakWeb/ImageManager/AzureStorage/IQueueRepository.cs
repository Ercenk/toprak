namespace ToprakWeb.ImageManager.AzureStorage
{
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.WindowsAzure.Storage.Queue;

    public interface IQueueRepository
    {
        Task AddMessageAsync(string queueName, CloudQueueMessage message, CancellationToken cancellationToken);

        Task<CloudQueueMessage> GetMessageAsync(string queueName, CancellationToken cancellationToken);

        Task DeleteMessageAsync(string queueName, string messageId, string popReceipt, CancellationToken cancellationToken);

        Task<int?> GetRemainingTutanakQueueLength(CancellationToken cancellationToken);
    }
}
