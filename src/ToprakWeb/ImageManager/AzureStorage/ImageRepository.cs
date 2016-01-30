namespace ToprakWeb.ImageManager.AzureStorage
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Framework.Logging;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Blob;
    using Microsoft.WindowsAzure.Storage.RetryPolicies;
    using Microsoft.WindowsAzure.Storage.Shared.Protocol;

    public class ImageRepository : IImageRepository
    {
        private readonly ILogger logger;
        private readonly CloudBlobContainer unprocessedBlobsContainer;
        private readonly CloudBlobContainer processedBlobsContainer;
        private readonly CloudBlobContainer stagingContainer;
        private readonly CloudBlobContainer weirdBlobContainer;

        private readonly CloudBlobClient blobClient;

        public ImageRepository(IStorageConnectionFactory connectionFactory, ILogger logger)
        {
            this.logger = logger;
            CloudStorageAccount account;
            if (connectionFactory.TryGetAccount(out account))
            {
                this.blobClient = account.CreateCloudBlobClient();
                this.unprocessedBlobsContainer = this.blobClient.GetContainerReference(ContainerNames.UnprocessedImagesContainerName);
                this.unprocessedBlobsContainer.CreateIfNotExists();

                this.processedBlobsContainer = this.blobClient.GetContainerReference(ContainerNames.ProcessedImagesContainerName);
                this.processedBlobsContainer.CreateIfNotExists();

                this.stagingContainer = this.blobClient.GetContainerReference(ContainerNames.StagingContainerName);
                this.stagingContainer.CreateIfNotExists();

                this.weirdBlobContainer = this.blobClient.GetContainerReference(ContainerNames.WeirdBlobContainerName);
                this.weirdBlobContainer.CreateIfNotExists();
            }
            else
            {
                this.logger.LogError("Cannot contruct blob repository, invalid connection.");
            }           
        }
        public Uri GetWriteUrl(string fileName)
        {
            var now = DateTimeOffset.UtcNow;
            var sharedAccessPolicy = new SharedAccessBlobPolicy()
            {
                Permissions = SharedAccessBlobPermissions.Write,
                SharedAccessStartTime = now,
                SharedAccessExpiryTime = now.AddMinutes(2)
            };
            var blob = this.stagingContainer.GetBlockBlobReference(fileName);
            var sharedAccessToken = blob.GetSharedAccessSignature(sharedAccessPolicy);
            var blobUri = blob.Uri.ToString();
            return new Uri($"{blobUri}{sharedAccessToken}");
        }

        public async Task SetCorsPropertiesAsync(CancellationToken cancellationToken)
        {
            var cors = new CorsRule();
            cors.AllowedOrigins.Add("*");
            cors.AllowedMethods = CorsHttpMethods.Put;
            cors.ExposedHeaders.Add("*");
            cors.AllowedHeaders.Add("*");
            cors.MaxAgeInSeconds = (int)TimeSpan.FromHours(1).TotalSeconds;

            var serviceProperties = await this.blobClient.GetServicePropertiesAsync(cancellationToken);
            serviceProperties.Cors.CorsRules.Clear();
            serviceProperties.Cors.CorsRules.Add(cors);
            await this.blobClient.SetServicePropertiesAsync(serviceProperties, cancellationToken);

        }

        public async Task<IEnumerable<IListBlobItem>> TransferFromStaging(bool rename, CancellationToken cancellationToken)
        {
            return await this.TransferBlobs(rename, this.stagingContainer, this.unprocessedBlobsContainer, cancellationToken);
        }

        public async Task TransferToProcessed(string imageName, CancellationToken cancellationToken)
        {
            await this.TransferBlob(this.unprocessedBlobsContainer, this.processedBlobsContainer, imageName, cancellationToken);
        }

        private async Task TransferBlob(
            CloudBlobContainer fromContainer,
            CloudBlobContainer toContainer,
            string blobName,
            CancellationToken cancellationToken)
        {
            var requestOptions = new BlobRequestOptions()
            {
                RetryPolicy = new LinearRetry(TimeSpan.FromSeconds(2), 5)
            };

            var destBlobRef = toContainer.GetBlobReference(blobName);
            var blob = fromContainer.GetBlobReference(blobName);

            await destBlobRef.StartCopyAsync(
                               blob.Uri,
                               AccessCondition.GenerateEmptyCondition(),
                               AccessCondition.GenerateEmptyCondition(),
                               requestOptions,
                               null,
                               cancellationToken);

            var allCopied = false;
            var copyFailed = false;

            do
            {
                var blobRef = await toContainer.GetBlobReferenceFromServerAsync(blobName, cancellationToken);
                if (blobRef.CopyState.Status == CopyStatus.Aborted || blobRef.CopyState.Status == CopyStatus.Failed)
                {
                    this.logger.LogError($"Cannot copy {blobRef.Uri}");
                    copyFailed = true;
                }

                allCopied = blobRef.CopyState.Status != CopyStatus.Pending;
            }
            while (!allCopied || copyFailed);

            await blob.DeleteAsync(cancellationToken);
        }

        private async Task<IEnumerable<IListBlobItem>> TransferBlobs(
            bool rename, 
            CloudBlobContainer fromContainer,
            CloudBlobContainer toContainer,
            CancellationToken cancellationToken)
        {
            var requestOptions = new BlobRequestOptions()
            {
                RetryPolicy = new LinearRetry(TimeSpan.FromSeconds(2), 5)
            };

            var leaseId = Guid.NewGuid().ToString();
            var leaseResult = string.Empty;
            var autoEvent = new AutoResetEvent(false);
            var waitEvent = new AutoResetEvent(false);

            var leaseTimer = new Timer(
                async s =>
                {
                    try
                    {
                        if (string.IsNullOrEmpty(leaseResult))
                        {
                            leaseResult =
                                await
                                fromContainer.AcquireLeaseAsync(
                                    TimeSpan.FromSeconds(60),
                                    leaseId,
                                    null,
                                    requestOptions,
                                    null,
                                    cancellationToken);
                            waitEvent.Set();
                        }
                        else
                        {
                            await
                                fromContainer.RenewLeaseAsync(
                                    AccessCondition.GenerateLeaseCondition(leaseId),
                                    requestOptions,
                                    null,
                                    cancellationToken);
                        }

                    }
                    catch (StorageException exception)
                    {
                        if (exception.RequestInformation.HttpStatusCode == (int)HttpStatusCode.Conflict)
                        {
                            this.logger.LogInformation("Staging container already has a lease.");
                        }
                    }
                },
                autoEvent,
                TimeSpan.FromSeconds(0),
                TimeSpan.FromSeconds(50));
            waitEvent.WaitOne();

            try
            {
                BlobContinuationToken token = null;
                var blobList = new List<CopySpec>();
                do
                {
                    var result = await fromContainer.ListBlobsSegmentedAsync(token, cancellationToken);
                    token = result.ContinuationToken;
                    blobList.AddRange(result.Results.OfType<CloudBlob>().Select(b => new CopySpec() {SourceBlob = b}));

                } while (token != null);

                // Copy
                var copiedBlobList = new List<CopySpec>();
                foreach (var blob in blobList)
                {
                    var srcBlobName = blob.SourceBlob.Uri.Segments[2];
                    var blobName = rename
                                       ? $"{Path.GetFileNameWithoutExtension(srcBlobName)}{Guid.NewGuid().ToString().Replace("-", "")}{Path.GetExtension(srcBlobName)}"
                                       : srcBlobName;
                    var destBlobRef = toContainer.GetBlobReference(blobName);
                    blob.DestBlob = destBlobRef;
                    try
                    {
                        await
                            destBlobRef.StartCopyAsync(
                                blob.SourceBlob.Uri,
                                AccessCondition.GenerateEmptyCondition(),
                                AccessCondition.GenerateEmptyCondition(),
                                requestOptions,
                                null,
                                cancellationToken);
                        copiedBlobList.Add(blob);
                    }
                    catch (Exception e)
                    {
                        this.logger.LogError($"Error while copying {blobName}. {e.ToString()}");
                    }
                }

                this.logger.LogInformation($"Started copying {copiedBlobList.Count} blobs");

                var blobsToRemove = new List<CopySpec>();
                var blobsToCheck = copiedBlobList.Select(b => b.SourceBlob.Uri.AbsoluteUri).ToList();

                do
                {
                    var withProperties = copiedBlobList.Select(b =>
                    {
                        b.DestBlob.FetchAttributes(AccessCondition.GenerateEmptyCondition(), requestOptions, null);
                        return b;
                    }).ToList();

                    foreach (var blob in withProperties)
                    {
                        if (blob.DestBlob.CopyState.Status == CopyStatus.Aborted
                            || blob.DestBlob.CopyState.Status == CopyStatus.Failed)
                        {
                            this.logger.LogError($"Cannot copy {blob.DestBlob.Uri}");
                            blobsToCheck.Remove(blob.SourceBlob.Uri.AbsoluteUri);
                        }

                        if (blob.DestBlob.CopyState.Status != CopyStatus.Success) continue;

                        blobsToRemove.Add(blob);
                        blobsToCheck.Remove(blob.SourceBlob.Uri.AbsoluteUri);
                    }
                }
                while (blobsToCheck.Any());

                this.logger.LogInformation($"{blobsToRemove.Count} blobs copied.");

                foreach (var blob in blobsToRemove)
                {
                    try
                    {
                        await
                            blob.SourceBlob.DeleteAsync(
                                DeleteSnapshotsOption.IncludeSnapshots,
                                AccessCondition.GenerateEmptyCondition(),
                                requestOptions,
                                null,
                                cancellationToken);
                        this.logger.LogInformation($"Deleted {blob.SourceBlob.Uri.AbsoluteUri}");
                    }
                    catch (StorageException e)
                    {
                        if (e.RequestInformation.HttpStatusCode == (int)HttpStatusCode.NotFound)
                        {
                            this.logger.LogInformation($"Blob not found {blob.SourceBlob.Uri}");
                        }
                        else
                        {
                            this.logger.LogError(e.ToString());
                        }
                    }
                    catch (Exception exception)
                    {
                        this.logger.LogError(exception.ToString());
                    }
                };
                leaseTimer.Dispose();

                await fromContainer.ReleaseLeaseAsync(AccessCondition.GenerateLeaseCondition(leaseId),
                    requestOptions, null, cancellationToken);

                this.logger.LogInformation($"{blobsToRemove.Count} blobs deleted.");

                return copiedBlobList.Where(b => b.DestBlob.CopyState.Status == CopyStatus.Success).Select(b => b.DestBlob);
            }
            catch (Exception exception)
            {
                this.logger.LogCritical(exception.ToString());
                return default(IEnumerable<IListBlobItem>);
            }
        }

        public Uri GetReadOnlyUrl(Uri blobUri, int? totalMinutes)
        {
            var now = DateTimeOffset.UtcNow;
            var sharedAccessPolicy = new SharedAccessBlobPolicy()
            {
                Permissions = SharedAccessBlobPermissions.Read,
                SharedAccessStartTime = now.AddMinutes(-2),
                SharedAccessExpiryTime = now.AddMinutes(totalMinutes ?? 5)
            };

            var containerName = blobUri.Segments[1].Replace("/", "");
            var blobName = blobUri.Segments[2].Replace("/", "");

            CloudBlobContainer container = null;
            switch (containerName)
            {
                case ContainerNames.StagingContainerName:
                    container = this.stagingContainer;
                    break;
                case ContainerNames.ProcessedImagesContainerName:
                    container = this.processedBlobsContainer;
                    break;
                case ContainerNames.UnprocessedImagesContainerName:
                    container = this.unprocessedBlobsContainer;
                    break;
            }

            if (container == null)
            {
                return default(Uri);
            }

            var blob = container.GetBlockBlobReference(blobName);
            var requestOptions = new BlobRequestOptions()
            {
                RetryPolicy = new LinearRetry(TimeSpan.FromSeconds(2), 5)
            };

            var sharedAccessToken = blob.GetSharedAccessSignature(sharedAccessPolicy);
            var readOnlyblobUri = blob.Uri.ToString();
            return new Uri($"{readOnlyblobUri}{sharedAccessToken}");
        }

        public Uri GetUnprocessedUri(string image)
        {
            return this.unprocessedBlobsContainer.GetBlobReference(image).Uri;
        }

        public async Task<IEnumerable<Uri>> GetRemainingTutanak(CancellationToken cancellationToken)
        {
            var requestOptions = new BlobRequestOptions()
            {
                RetryPolicy = new LinearRetry(TimeSpan.FromSeconds(2), 5)
            };

            var blobList = new List<IListBlobItem>();

            BlobContinuationToken token = null;
                do
                {
                    var result =
                        await
                        this.unprocessedBlobsContainer.ListBlobsSegmentedAsync(
                            null,
                            false,
                            BlobListingDetails.Copy,
                            null,
                            token,
                            requestOptions,
                            null,
                            cancellationToken);
                    token = result.ContinuationToken;
                blobList.AddRange(result.Results);

                } while (token != null);
            return blobList.Select(b => this.GetReadOnlyUrl(b.Uri, null));

        }

        public async Task TransferToWeird(string image, CancellationToken cancellationToken)
        {
            await this.TransferBlob(this.unprocessedBlobsContainer, this.weirdBlobContainer, image, cancellationToken);
        }

        public async Task<Uri> GetReadOnlyUrlForImage(string image, int? totalMinutes, CancellationToken cancellationToken)
        {
            Uri blobUri;
            
            if (Uri.TryCreate(image, UriKind.Absolute, out blobUri))
            {
                var containerName = blobUri.Segments[1].Replace("/", "");
                var blobName = blobUri.Segments[2].Replace("/", "");
                image = blobName;
            }

            var requestOptions = new BlobRequestOptions()
            {
                RetryPolicy = new LinearRetry(TimeSpan.FromSeconds(2), 5)
            };
            var blobReference = this.processedBlobsContainer.GetBlobReference(image);
            if (await blobReference.ExistsAsync(requestOptions, null, cancellationToken))
            {
                return this.GetReadOnlyUrl(blobReference.Uri, null);
            }
            blobReference = this.unprocessedBlobsContainer.GetBlockBlobReference(image);
            if (await blobReference.ExistsAsync(requestOptions, null, cancellationToken))
            {
                return this.GetReadOnlyUrl(blobReference.Uri, null);
            }
            return default(Uri);
        }
    }

    internal class CopySpec
    {
        public CloudBlob SourceBlob { get; set; }
        public CloudBlob DestBlob { get; set; }
    }
}