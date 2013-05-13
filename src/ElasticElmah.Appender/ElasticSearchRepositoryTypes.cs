using System;
using System.Collections.Generic;
using Nest;
using log4net.Core;
using log4net.Util;

namespace ElasticElmah.Appender
{
    public partial class ElasticSearchRepository
    {
        public class LocInfo
        {
            public string ClassName { get; set; }
            public string FileName { get; set; }
            public string LineNumber { get; set; }
            public string MethodName { get; set; }
        }

        [ElasticType(Name = "LoggingEvent")]
        public class LogEvent
        {
            public LogEvent()
            {
                LocationInfo = new LocInfo();
            }
            public string LoggerName { get; set; }
            public string Level { get; set; }
            public string Message { get; set; }
            public string ThreadName { get; set; }
            public DateTime TimeStamp { get; set; }
            public LocInfo LocationInfo { get; set; }
            public string UserName { get; set; }
            public Dictionary<string, string> Properties { get; set; }
            public string ExceptionString { get; set; }
            public string Domain { get; set; }
            public string Identity { get; set; }
        }

        private static LoggingEventData Map(LogEvent l)
        {
            var d = new LoggingEventData();
            d.LoggerName = l.LoggerName;
            d.Level = _log.Logger.Repository.LevelMap[l.Level];
            d.Message = l.Message;
            d.ThreadName = l.ThreadName;
            d.TimeStamp = l.TimeStamp;
            if (l.LocationInfo != null)
            {
                var i = l.LocationInfo;
                d.LocationInfo = new LocationInfo(i.ClassName, i.MethodName, i.FileName, i.LineNumber);
            }
            d.UserName = l.UserName;
            if (l.Properties != null)
                d.Properties = Map(l.Properties);
            d.ExceptionString = l.ExceptionString;
            d.Domain = l.Domain;
            d.Identity = l.Identity;
            return d;
        }

        private LogEvent Map(LoggingEvent l)
        {
            var d = new LogEvent();
            d.LoggerName = l.LoggerName;
            d.Level = l.Level.Name;
            d.Message = l.RenderedMessage;
            d.ThreadName = l.ThreadName;
            d.TimeStamp = l.TimeStamp;
            if (l.LocationInformation != null)
            {
                var i = l.LocationInformation;
                var di = d.LocationInfo;
                di.ClassName = i.ClassName;
                di.FileName = i.FileName;
                di.LineNumber = i.LineNumber;
                di.MethodName = i.MethodName;
            }
            d.UserName = l.UserName;
            if (l.Properties != null)
                d.Properties = Map(l.Properties);
            d.ExceptionString = l.GetExceptionString();
            d.Domain = l.Domain;
            d.Identity = l.Identity;
            return d;
        }

        private Dictionary<string, string> Map(PropertiesDictionary p)
        {
            var d = new Dictionary<string, string>();
            foreach (var key in p.GetKeys())
            {
                d.Add(key, p[key].ToString());
            }
            return d;
        }

        private static PropertiesDictionary Map(Dictionary<string, string> dictionary)
        {
            var dic = new PropertiesDictionary();
            foreach (var item in dictionary)
            {
                dic[item.Key] = item.Value;
            }
            return dic;
        }


    }
}
