namespace ToprakWeb.ImageManager.Model
{
    public class TemsilcilikSpec : IDbResult
    {
        public int Id { get; set; }
        public string Ulke { get; set; }

        public string Temsilcilik { get; set; }
    }
}