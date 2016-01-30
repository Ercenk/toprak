namespace ToprakWeb.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using ImageManager;
    using ImageManager.Model;
    using Microsoft.AspNet.Authorization;
    using Microsoft.AspNet.Mvc;
    using Microsoft.Framework.Logging;

    using ToprakWeb.Authorization;
    using ToprakWeb.ResultsRepository;

    [Route("api/v1/[controller]")]
    [Authorize(Policy = "MustBeAdmin", ActiveAuthenticationSchemes = ToprakWebOptions.AuthScheme)]    
    public class AdminController: Controller
    {
        private readonly IImageManagerService imageManagerService;

        private readonly IResultsRepository resultsRepository;

        private readonly ILogger logger;

        public AdminController(IImageManagerService imageManagerService, IResultsRepository resultsRepository, ILogger logger)
        {
            this.imageManagerService = imageManagerService;
            this.resultsRepository = resultsRepository;
            this.logger = logger;
        }

        //[HttpGet("{fileName}", Name="GetUploadUrl")]
        [Route("[action]/{fileName}")]
        public UrlResult GetUploadUrl(string fileName)
        {
            var result = this.imageManagerService.GetWriteUrl(fileName, Request.HttpContext.Connection.RemoteIpAddress.ToString(), "ben");

            return new UrlResult(){Value = result};
        }

        [HttpGet]
        [Route("[action]")]
        [ResponseCache(NoStore = true)]
        public async Task SetupUpload(CancellationToken cancellationToken)
        {
            await this.imageManagerService.SetupUpload(cancellationToken);
        }

        [HttpPost]
        [Route("[action]")]
        public async Task UploadDone([FromBody] string name, string fileName)
        {
            await Task.FromResult<string>("Dummy action");

            Debug.WriteLine("here");
        }

        [HttpGet]
        [Route("[action]")]
        [ResponseCache(NoStore = true)]
        public async Task TransferFromStaging(CancellationToken cancellationToken)
        {
            await this.imageManagerService.SignalLoadImagesAsync(cancellationToken);
        }

        [HttpGet]
        [Route("[action]")]
        [ResponseCache(NoStore = true)]
        public async Task<UriResult> RemainingTutanak(CancellationToken cancellationToken)
        {
            var result = new UriResult() { Uris = (await this.imageManagerService.GetRemainingTutanak(cancellationToken)).Select(u => new UriElement() { Uri = u }) };
            var queueLength = await this.imageManagerService.GetRemainingTutanakQueueLength(cancellationToken);
            result.QueueLength = queueLength ?? 0;
            return result;
        }

        [HttpGet]
        [Route("[action]")]
        [ResponseCache(NoStore = true)]
        public async Task<TopSeenResult> TopSeenTutanak(CancellationToken cancellationToken)
        {
            var topSeenImages = await this.resultsRepository.GetTopSeenImages(cancellationToken);
            var topSeenImagesWithUri = new List<ImageReadCount>();
            foreach (var imageResult in topSeenImages)
            {
                topSeenImagesWithUri.Add(new ImageReadCount()
                {
                    Image = (await this.imageManagerService.GetReadOnlyUrlForImage(imageResult.Image, 30, cancellationToken)).AbsoluteUri,
                    Count = imageResult.Count
                });
            }

            return new TopSeenResult() { Result = topSeenImagesWithUri };
        }

        [HttpGet]
        [Route("[action]")]
        [ResponseCache(NoStore = true)]
        public async Task<TopSeenResult> UnreadableTutanak(CancellationToken cancellationToken)
        {
            var topSeenImages = await this.resultsRepository.GetUnreadableImages(cancellationToken);
            var topSeenImagesWithUri = new List<ImageReadCount>();
            foreach (var imageResult in topSeenImages)
            {
                topSeenImagesWithUri.Add(new ImageReadCount()
                {
                    Image = (await this.imageManagerService.GetReadOnlyUrlForImage(imageResult.Image, 30, cancellationToken)).AbsoluteUri,
                    Count = imageResult.Count
                });
            }

            return new TopSeenResult() { Result = topSeenImagesWithUri };
        }

        [HttpGet]
        [Route("[action]")]
        [ResponseCache(NoStore = true)]
        public async Task<GonulluResult> TopGonullu(CancellationToken cancellationToken)
        {
            return new GonulluResult() { Users = await this.resultsRepository.GetTopGonullu(cancellationToken) };
        }

        [HttpGet]
        [Route("[action]")]
        [ResponseCache(NoStore = true)]
        public async Task AddToCirculation(CancellationToken cancellationToken)
        {
            await this.imageManagerService.AddToCirculation(false, cancellationToken);
        }

        [HttpGet]
        [Route("[action]")]
        [ResponseCache(NoStore = true)]
        public async Task<LocationStatsResult> GetUserLocations(CancellationToken cancellationToken)
        {
            return new LocationStatsResult()
                       {
                           LocationStats =
                               await this.resultsRepository.GetUserLocations(cancellationToken)
                       };
        }

    }

    public class LocationStatsResult
    {
        public IEnumerable<LocationStats> LocationStats { get; set; }
    }

    public class GonulluResult
    {
        public IEnumerable<User> Users { get; set; }
    }

    public class TopSeenResult
    {
        public IEnumerable<ImageReadCount> Result { get; set; }
    }

    public class UriResult
    {
        public IEnumerable<UriElement> Uris { get; set; }

        public int QueueLength { get; set; }
    }

    public class UriElement
    {
        public Uri Uri { get; set; }
        
    }
}
