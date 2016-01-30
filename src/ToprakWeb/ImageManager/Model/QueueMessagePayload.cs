namespace ToprakWeb.ImageManager.Model
{
    using Newtonsoft.Json;

    public class QueueMessagePayload
    {
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}   