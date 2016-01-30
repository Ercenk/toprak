namespace ToprakWeb.ImageManager
{
    using System.Threading.Tasks;
    using Model;

    public interface IUserManagerService
    {
        Task Update(User user);

        Task<UserStat> GetStat(string email);
    }
}
