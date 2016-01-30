namespace ToprakWeb.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using ImageManager;

    using Microsoft.AspNet.Mvc;
    using System.Threading;

    using ImageManager.Model;

    using Microsoft.ApplicationInsights.AspNet.Extensions;
    using Microsoft.AspNet.Authorization;
    using Microsoft.Framework.Logging;

    using ToprakWeb.Authorization;
    using ToprakWeb.ResultsRepository;

    [Route("api/v1/[controller]")]
    [Authorize(ActiveAuthenticationSchemes = ToprakWebOptions.AuthScheme)]
    public class TutanakController : Controller
    {
        private readonly IImageManagerService imageManagerService;

        private readonly IResultsRepository resultsRepository;

        private readonly ILogger logger;

        public TutanakController(IImageManagerService imageManagerService, IResultsRepository resultsRepository, ILogger logger)
        {
            this.imageManagerService = imageManagerService;
            this.resultsRepository = resultsRepository;
            this.logger = logger;
        }

        // POST api/values
        [HttpPost]
        [Route("[action]")]
        public async Task Kaydet([FromBody]GorulmusTutanakMesaji tutanakData, CancellationToken cancellationToken)
        {
            var ipAddress = string.Empty;
            try
            {
                ipAddress = Request.HttpContext.Connection.RemoteIpAddress.ToString();
            }
            catch (Exception exception)
            {
                this.logger.LogError(exception.ToString());
            }
            tutanakData.ClientIp = ipAddress;
            tutanakData.WhenSeen = DateTimeOffset.UtcNow;
            var userRecord = new User(Request.HttpContext.User, tutanakData.ClientIp);
            if (string.IsNullOrEmpty(userRecord.Email) || userRecord.Email == "noemail")
            {
                userRecord.Email = string.IsNullOrEmpty(tutanakData.SeenBy) ? "noemail" : tutanakData.SeenBy;
            }

            if (tutanakData.Okunabiliyor)
            {
                tutanakData.SandikKurulNo = tutanakData.SandikKurulNo.ToUpper().Replace(" ", "");
            }

            await this.imageManagerService.ProcessImageAsync(new TutanakDataEnvelope(tutanakData, userRecord), cancellationToken);
        }

        [HttpGet]
        [Route("[action]")]
        [ResponseCache(NoStore = true)]
        public async Task<UrlResult> GetImageUri(CancellationToken token)
        {
            var result =  await this.imageManagerService.GetFirstImageAsync(token);

            return result;
        }

        [HttpGet]
        [Route("[action]")]
        [ResponseCache(NoStore = true)]
        public async Task<UserStat> GetUserStats(CancellationToken token)
        {
            var email = "noemail";
            if (Request.HttpContext.User.Claims.Any(c => c.Type == "Email"))
            {
                email = Request.HttpContext.User.Claims.First(c => c.Type == "Email").Value;
            }

            var result = await this.resultsRepository.GetUserStats(email);

            return result;
        }

        [HttpGet]
        [Route("[action]")]
        [ResponseCache(NoStore = true)]
        public async Task<UlkeVeTemsilciliklerResult> GetTemsilcilikler(CancellationToken token)
        {
            var result = await this.resultsRepository.GetTemsilcilikler(token);
            var temsilcilikler =
                result.GroupBy(t => t.Ulke)
                    .Select(g => new UlkeVeTemsilcilikler() { UlkeAdi = g.Key, Temsilcilikler = g });

            return new UlkeVeTemsilciliklerResult() {Value = temsilcilikler};
        }
    }

    public class UlkeVeTemsilciliklerResult
    {
        public IEnumerable<UlkeVeTemsilcilikler> Value { get; set; }
    }

    public class UlkeVeTemsilcilikler   
    {
        public IEnumerable<TemsilcilikSpec> Temsilcilikler { get; set; }

        public string UlkeAdi { get; set; }
    }
}
