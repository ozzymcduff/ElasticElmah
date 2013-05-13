using System;
using System.Collections.Generic;
using Nest;

namespace ElasticElmah.Appender.Storage
{
    [ElasticType(Name = "LoggingEvent")]
    public class LogEvent
    {
        public LogEvent()
        {
            LocationInfo = new LogEventLocation();
        }
        public string LoggerName { get; set; }
        public string Level { get; set; }
        public string Message { get; set; }
        public string ThreadName { get; set; }
        public DateTime TimeStamp { get; set; }
        public LogEventLocation LocationInfo { get; set; }
        public string UserName { get; set; }
        public Dictionary<string, string> Properties { get; set; }
        public string ExceptionString { get; set; }
        public string Domain { get; set; }
        public string Identity { get; set; }
    }
}