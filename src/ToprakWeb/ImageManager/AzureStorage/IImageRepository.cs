namespace ToprakWeb.ImageManager.AzureStorage
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.WindowsAzure.Storage.Blob;

    public interface IImageRepository
    {
        Uri GetWriteUrl(string fileName);

        Task SetCorsPropertiesAsync(CancellationToken cancellationToken);

        Task<IEnumerable<IListBlobItem>> TransferFromStaging(bool rename, CancellationToken cancellationToken);

        Task TransferToProcessed(string imageName, CancellationToken cancellationToken);

        Uri GetReadOnlyUrl(Uri blobUri, int? totalMinutes);

        Uri GetUnprocessedUri(string image);

        Task<IEnumerable<Uri>>  GetRemainingTutanak(CancellationToken cancellationToken);

        Task<Uri> GetReadOnlyUrlForImage(string image, int? totalMinutes, CancellationToken cancellationToken);
    }
}
