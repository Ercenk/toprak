namespace ToprakWeb.ResultsRepository
{
    using System;

    using Newtonsoft.Json;

    public class UserLocation
    {
        [JsonProperty(PropertyName = "ip")]
        public string Ip { get; set; }

        [JsonProperty(PropertyName = "country_name")]
        public string CountryName { get; set; }

        [JsonProperty(PropertyName = "region_name")]
        public string Region { get; set; }

        [JsonProperty(PropertyName = "city")]
        public string City { get; set; }


        [JsonProperty(PropertyName = "latitude")]
        public string Latitude { get; set; }


        [JsonProperty(PropertyName = "longitude")]
        public string Longitude { get; set; }

        public static UserLocation Build(string result)
        {
            return JsonConvert.DeserializeObject<UserLocation>(result);
        }
    }
}