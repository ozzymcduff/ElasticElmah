using Makrill;
using Newtonsoft.Json;

namespace ElasticElmah.Appender.Tests
{
    public class WrappedNewtonsoft : IJsonSerializer
    {
        public T Deserialize<T>(string val)
        {
            var settings = new JsonSerializerSettings();
            settings.Converters.Add(new ToStandardConverter());
            return JsonConvert.DeserializeObject<T>(val, settings);
        }

        public string Serialize<T>(T obj)
        {
            return JsonConvert.SerializeObject(obj);
        }
    }
}
