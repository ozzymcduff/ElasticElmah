using System.Web.Script.Serialization;

namespace ElasticElmah.Appender
{
    public class DefaultJsonSerializer:IJsonSerializer
    {
        private JavaScriptSerializer wrapped;
        public DefaultJsonSerializer()
        {
            wrapped = new JavaScriptSerializer();
        }
        public T Deserialize<T>(string val) 
        {
            return wrapped.Deserialize<T>(val);
        }
        public string Serialize<T>(T obj) 
        {
            return wrapped.Serialize(obj);
        }
    }
}
