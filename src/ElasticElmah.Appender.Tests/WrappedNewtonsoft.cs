using Makrill;
using Newtonsoft.Json;
using JsonConvert = Newtonsoft.Json.JsonConvert;
#if NET20
namespace ElasticElmah.Appender.net20.Tests
#else
namespace ElasticElmah.Appender.Tests
#endif
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
