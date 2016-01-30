namespace ToprakWeb.ImageManager
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AzureStorage;
    using Microsoft.Framework.Logging;
    using Microsoft.WindowsAzure.Storage.Queue;
    using Model;

    public class ImageManagerService : IImageManagerService
    {
        private readonly IImageRepository imageRepository;

        private readonly ILogger logger;

        private readonly IQueueRepository queueRepository;

        public ImageManagerService(IQueueRepository queueRepository, IImageRepository imageRepository, ILogger logger)
        {
            this.queueRepository = queueRepository;
            this.imageRepository = imageRepository;
            this.logger = logger;
        }

        public ImageManagerService(StorageConnectionFactory connectionFactory, ILogger logger) : this(new QueueRepository(connectionFactory, logger), new ImageRepository(connectionFactory, logger), logger)
        {
        }

        public async Task<UrlResult> GetFirstImageAsync(CancellationToken cancelToken)
        {
            var message = await this.queueRepository.GetMessageAsync(QueueNames.ToBeProcessed, cancelToken);
           
            var blobUri = message == null ? null : new Uri(message.AsString);
            Uri imageUri = null;

            if (blobUri != null)
            {
                imageUri = await this.imageRepository.GetReadOnlyUrlForImage(message.AsString, null, cancelToken);
                if (imageUri == null)
                {
                    await
                        this.queueRepository.DeleteMessageAsync(QueueNames.ToBeProcessed, message.Id, message.PopReceipt,
                            cancelToken);
                }
            }

            return blobUri == null || imageUri == null
                ? default(UrlResult)
                : new UrlResult()
                {
                    Value = await this.imageRepository.GetReadOnlyUrlForImage(message.AsString, null, cancelToken),
                    MessageId = message.Id,
                    Receipt = message.PopReceipt
                };
        }

        public async Task ProcessImageAsync(TutanakDataEnvelope envelope, CancellationToken cancelToken)
        {
            try
            {
                await
                    this.queueRepository.DeleteMessageAsync(QueueNames.ToBeProcessed, envelope.TutanakData.MessageId,
                        envelope.TutanakData.Receipt, cancelToken);
                await this.queueRepository.AddMessageAsync(QueueNames.Seen, new CloudQueueMessage(envelope.ToString()), cancelToken);
            }
            catch (Exception e)
            {
                this.logger.LogError(e.ToString());
            }
            
        }

        public Uri GetWriteUrl(string fileName, string clientIp, string userName)
        {
            var newFileName =
                $"{Path.GetFileNameWithoutExtension(fileName)}{Guid.NewGuid().ToString().Replace("-", "")}{Path.GetExtension(fileName)}";
            this.logger.LogInformation($"SAS Uri is generated for file {fileName} by {userName} on IP {clientIp} at {DateTimeOffset.UtcNow}. New blob name is {newFileName}");
            return this.imageRepository.GetWriteUrl(newFileName);
        }

        public async Task SetupUpload(CancellationToken cancellationToken)
        {
            await this.imageRepository.SetCorsPropertiesAsync(cancellationToken);
        }
        
        public async Task SignalLoadImagesAsync(CancellationToken cancellationToken)
        {
            await
                this.queueRepository.AddMessageAsync(QueueNames.NewImages, new CloudQueueMessage("New"),
                    cancellationToken);
        }

        public async Task<IEnumerable<Uri>>  GetRemainingTutanak(CancellationToken cancellationToken)
        {
            return await this.imageRepository.GetRemainingTutanak(cancellationToken);
        }

        public async Task<Uri> GetReadOnlyUrlForImage(string image, int totalMinutes, CancellationToken cancellationToken)
        {
            return await this.imageRepository.GetReadOnlyUrlForImage(image, totalMinutes, cancellationToken);
        }

        public async Task<int?> GetRemainingTutanakQueueLength(CancellationToken cancellationToken)
        {
            return await this.queueRepository.GetRemainingTutanakQueueLength(cancellationToken);
        }

        public async Task AddToCirculation(bool rename, CancellationToken cancellationToken)
        {
            var newImages = (await this.imageRepository.TransferFromStaging(rename, cancellationToken));

            if (newImages == null)
            {
                return;
            }

            var newImagesList = newImages.ToList();
            foreach (var image in newImagesList)
            {
                await
                    this.queueRepository.AddMessageAsync(
                        QueueNames.ToBeProcessed,
                        new CloudQueueMessage(image.Uri.AbsoluteUri),
                        cancellationToken);
            }

            this.logger.LogInformation($"Added {newImagesList.Count} images.");
        }
    }
}