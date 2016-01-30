namespace ToprakWeb.ImageManager.Model
{
    using Newtonsoft.Json;

    public class TutanakDataEnvelope : QueueMessagePayload
    {
        public GorulmusTutanakMesaji TutanakData { get; set; }
        public User UserRecord { get; set; }

        public TutanakDataEnvelope(GorulmusTutanakMesaji tutanakData, User userRecord)
        {
            TutanakData = tutanakData;
            UserRecord = userRecord;
        }

        public static TutanakDataEnvelope GorulmusTutanakMesajiFactory(string jsonRepresentation)
        {
            return JsonConvert.DeserializeObject<TutanakDataEnvelope>(jsonRepresentation);
        }
    }
}