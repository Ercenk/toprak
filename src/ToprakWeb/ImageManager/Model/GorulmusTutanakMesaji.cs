namespace ToprakWeb.ImageManager.Model
{
    using System;
    using global::ImageManager.Model;
    using Newtonsoft.Json;

    public class GorulmusTutanakMesaji
    {
        public GorulmusTutanakMesaji()
        {
        }

        [NoCompare]
        public DateTimeOffset WhenSeen { get; set; }

        [NoCompare]
        public string SeenBy { get; set; }

        [NoCompare]
        public string ClientIp { get; set; }

        [NoCompare]
        public string Image { get; set; }

        [NoCompare]
        public string MessageId { get; set; }

        [NoCompare]
        public string Receipt { get; set; }


        public bool Okunabiliyor { get; set; }

        public int Temsilcilik { get; set; }

        public string SandikKurulNo { get; set; }

        public string SandikSayimKurulNo { get; set; }

        public int SandikSecmenSayisi1 { get; set; }

        public int OyKullanan2 { get; set; }

        public int SandiktanCikanZarf3 { get; set; }

        public int SecimTorbasindanCikanZarf4 { get; set; }
        public int YokEdilenZarf5 { get; set; }

        public int GecerliZarf6 { get; set; }

        public int ItirazOlmadanGecerliOy7 { get; set; }

        public int ItirazUzerineGecerliOy8 { get; set; }

        public int GecerliOyToplam9 { get; set; }

        public int GecersizZarf10 { get; set; }

        public int BosZarf11 { get; set; }

        public int GercersizSayilanOy12 { get; set; }

        public int HesabaKatilmayanOy13 { get; set; }
        public int GecersizSayilanVeyaHesabaKatilmayanOy14 { get; set; }

        public int MHP { get; set; }

        public int HDP { get; set; }

        public int SP { get; set; }

        public int CHP { get; set; }

        public int AKP { get; set; }

        public int Others { get; set; }

        public bool BaskanImzasi { get; set; }

        public bool UyeImzasi { get; set; }

        public bool Muhur { get; set; }
    }
}