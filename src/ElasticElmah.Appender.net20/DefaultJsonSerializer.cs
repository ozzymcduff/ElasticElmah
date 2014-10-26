using System.Web.Script.Serialization;

namespace ElasticElmah.Appender
{
    public class DefaultJsonSerializer:IJsonSerializer
    {
        private readonly JavaScriptSerializer _wrapped;
        public DefaultJsonSerializer()
        {
            _wrapped = new JavaScriptSerializer();
        }
        public T Deserialize<T>(string val) 
        {
            return _wrapped.Deserialize<T>(val);
        }
        public string Serialize<T>(T obj) 
        {
            return _wrapped.Serialize(obj);
        }
    }
}
