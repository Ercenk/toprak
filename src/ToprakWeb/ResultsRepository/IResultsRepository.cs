namespace ToprakWeb.ResultsRepository
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    using ToprakWeb.ImageManager;
    using ToprakWeb.ImageManager.Model;

    public interface IResultsRepository
    {
        Task<IEnumerable<GorulmusTutanakMesaji>> GetTutanakResultsAsync(string imageId);

        Task<UserStat> GetUserStats(string email);

        Task RecordResult(TutanakDataEnvelope envelope);

        Task<IEnumerable<TemsilcilikSpec>> GetTemsilcilikler(CancellationToken cancellationToken);

        Task RecordReadSuccessResult(TutanakDataEnvelope envelope);

        Task<IEnumerable<ImageReadCount>> GetTopSeenImages(CancellationToken cancellationToken);

        Task<IEnumerable<User>> GetTopGonullu(CancellationToken cancellationToken);
        Task<IEnumerable<ImageReadCount>> GetUnreadableImages(CancellationToken cancellationToken);

        Task<IEnumerable<LocationStats>> GetUserLocations(CancellationToken cancellationToken);
    }
}