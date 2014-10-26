namespace ElasticElmah.Appender
{
    public interface IJsonSerializer
    {
        T Deserialize<T>(string val);
        string Serialize<T>(T obj);
    }
}
