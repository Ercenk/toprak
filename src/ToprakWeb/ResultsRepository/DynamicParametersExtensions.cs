
namespace ToprakWeb.ResultsRepository
{
    using Dapper;

    using ToprakWeb.ImageManager.Model;

    public static class DynamicParametersExtensions
    {
        public static DynamicParameters AddUserInfo(this DynamicParameters parameters, User userInfo)
        {
            parameters.Add("@Email", userInfo.Email);
            parameters.Add("@FirstName", userInfo.FirstName);
            parameters.Add("@LastName", userInfo.LastName);
            parameters.Add("@TotalResponses", userInfo.TotalResponses);
            parameters.Add("@Ip", userInfo.Ip);

            return parameters;
        }

        public static DynamicParameters AddTutanakInfo(this DynamicParameters parameters, GorulmusTutanakMesaji tutanak)
        {
            parameters.Add("@WhenSeen", tutanak.WhenSeen);
            parameters.Add("@SeenBy", tutanak.SeenBy);
            parameters.Add("@ClientIp", tutanak.ClientIp);
            parameters.Add("@Image", tutanak.Image);
            parameters.Add("@Okunabiliyor", tutanak.Okunabiliyor);
            parameters.Add("@Temsilcilik", tutanak.Temsilcilik);
            parameters.Add("@SandikKurulNo", tutanak.SandikKurulNo);
            parameters.Add("@SandikSayimKurulNo", tutanak.SandikSayimKurulNo);
            parameters.Add("@SandikSecmenSayisi1", tutanak.SandikSecmenSayisi1);
            parameters.Add("@OyKullanan2", tutanak.OyKullanan2);
            parameters.Add("@SandiktanCikanZarf3", tutanak.SandiktanCikanZarf3);
            parameters.Add("@SecimTorbasindanCikanZarf4", tutanak.SecimTorbasindanCikanZarf4);
            parameters.Add("@YokEdilenZarf5", tutanak.YokEdilenZarf5);
            parameters.Add("@GecerliZarf6", tutanak.GecerliZarf6);
            parameters.Add("@ItirazOlmadanGecerliOy7", tutanak.ItirazOlmadanGecerliOy7);
            parameters.Add("@ItirazUzerineGecerliOy8", tutanak.ItirazUzerineGecerliOy8);
            parameters.Add("@GecerliOyToplam9", tutanak.GecerliOyToplam9);
            parameters.Add("@GecersizZarf10", tutanak.GecersizZarf10);
            parameters.Add("@BosZarf11", tutanak.BosZarf11);
            parameters.Add("@GercersizSayilanOy12", tutanak.GercersizSayilanOy12);
            parameters.Add("@HesabaKatilmayanOy13", tutanak.HesabaKatilmayanOy13);
            parameters.Add("@GecersizSayilanVeyaHesabaKatilmayanOy14", tutanak.GecersizSayilanVeyaHesabaKatilmayanOy14);
            parameters.Add("@MHP", tutanak.MHP);
            parameters.Add("@HDP", tutanak.HDP);
            parameters.Add("@SP", tutanak.SP);
            parameters.Add("@CHP", tutanak.CHP);
            parameters.Add("@AKP", tutanak.AKP);
            parameters.Add("@Others", tutanak.Others);
            parameters.Add("@BaskanImzasi", tutanak.BaskanImzasi);
            parameters.Add("@UyeImzasi", tutanak.UyeImzasi);
            parameters.Add("@Muhur", tutanak.Muhur);

            return parameters;
        }

        public static DynamicParameters AddLocation(this DynamicParameters parameters, UserLocation location)
        {
            parameters.Add("@Country", location.CountryName);
            parameters.Add("@Region", location.Region);
            parameters.Add("@City", location.City);
            parameters.Add("@Latitude", location.Latitude);
            parameters.Add("@Longitude", location.Longitude);

            return parameters;
        }
    }
}
