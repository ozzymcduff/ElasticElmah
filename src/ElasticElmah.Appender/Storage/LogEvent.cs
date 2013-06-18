using System.Collections.Generic;

namespace ElasticElmah.Appender.Storage
{
    public class LogEvent
    {
        public LogEvent()
        {
            locationInfo = new LogEventLocation();
        }
        public string loggerName { get; set; }
        public string level { get; set; }
        public string message { get; set; }
        public string threadName { get; set; }
        public string timeStamp { get; set; }
        public LogEventLocation locationInfo { get; set; }
        public string userName { get; set; }
        public Dictionary<string, object> properties { get; set; }
        public string exceptionString { get; set; }
        public string domain { get; set; }
        public string identity { get; set; }
    }
}