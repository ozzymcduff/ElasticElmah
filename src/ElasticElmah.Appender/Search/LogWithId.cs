using log4net.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ElasticElmah.Appender.Search
{
    public class LogWithId
    {
        public readonly string Id;
        public readonly LoggingEventData Data;

        public LogWithId(string id, LoggingEventData data)
        {
            this.Id = id;
            this.Data = data;
        }
    }
}
