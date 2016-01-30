namespace ToprakWeb.ResultsRepository
{
    using ToprakWeb.ImageManager.Model;

    public class LocationStats: IDbResult
    {
        public string Country { get; set; }

        public string City { get; set; }

        public decimal Longitude { get; set; }
        public decimal Latitude { get; set; }
        public int Analiz { get; set; }

    }
}