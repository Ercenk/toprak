namespace ToprakWeb.ImageManager
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Model;

    public interface IImageManagerService
    {
       Task<UrlResult> GetFirstImageAsync(CancellationToken cancelToken);

        Task ProcessImageAsync(TutanakDataEnvelope message, CancellationToken cancelToken);

        Uri GetWriteUrl(string fileName, string clientIp, string userName);

        Task SetupUpload(CancellationToken cancellationToken);

        Task SignalLoadImagesAsync(CancellationToken cancellationToken);

        Task<IEnumerable<Uri>> GetRemainingTutanak(CancellationToken cancellationToken);

        Task<Uri> GetReadOnlyUrlForImage(string image, int totalMinutes, CancellationToken cancellationToken);

        Task<int?> GetRemainingTutanakQueueLength(CancellationToken cancellationToken);

        Task AddToCirculation(Boolean rename, CancellationToken cancellationToken);
    }
}
