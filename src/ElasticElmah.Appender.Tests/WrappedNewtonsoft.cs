using Newtonsoft.Json;
namespace ElasticElmah.Appender
{
    public class WrappedNewtonsoft : IJsonSerializer
    {
        public T Deserialize<T>(string val)
        {
            return JsonConvert.DeserializeObject<T>(val);
        }

        public string Serialize<T>(T obj)
        {
            return JsonConvert.SerializeObject(obj);
        }
    }
}
