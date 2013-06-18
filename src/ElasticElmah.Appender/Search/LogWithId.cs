using log4net.Core;

namespace ElasticElmah.Appender.Search
{
    public class LogWithId
    {
        public readonly string Id;
        public readonly LoggingEventData Data;

        public LogWithId(string id, LoggingEventData data)
        {
            Id = id;
            Data = data;
        }
    }
}
