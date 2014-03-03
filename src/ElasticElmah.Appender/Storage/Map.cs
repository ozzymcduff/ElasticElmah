using System.Collections.Generic;
using System.Reflection;
using log4net;
using log4net.Core;
using log4net.Util;
using System;

namespace ElasticElmah.Appender.Storage
{
    public class Map
    {
        public static LoggingEventData To(LogEvent l)
        {
            var d = new LoggingEventData
                        {
                            LoggerName = l.loggerName,
                            Level = _log.Logger.Repository.LevelMap[l.level??"ERROR"],
                            Message = l.message,
                            ThreadName = l.threadName,
                            TimeStamp = string.IsNullOrEmpty(l.timeStamp)? DateTime.Now: DateTime.Parse(l.timeStamp),
                            UserName = l.userName,
                            ExceptionString = l.exceptionString,
                            Domain = l.domain,
                            Identity = l.identity,
                        };
            if (l.properties != null)
                d.Properties = To(l.properties);

            if (l.locationInfo != null)
            {
                var i = l.locationInfo;
                d.LocationInfo = new LocationInfo(i.className, i.methodName, i.fileName, i.lineNumber);
            }
            return d;
        }

        public static string To(DateTime datetime)
        {
            //: 2013-05-14T20:41:01.2255267+02:00
            return datetime.ToString("yyyy-MM-ddTHH:mm:ss.fffffffzzz");
        }

        public static LogEvent To(LoggingEvent l)
        {
            var d = new LogEvent
                        {
                            loggerName = l.LoggerName,
                            level = l.Level!=null ? l.Level.Name : string.Empty,
                            message = l.RenderedMessage,
                            threadName = l.ThreadName,
                            timeStamp = To(l.TimeStamp),
                            userName = l.UserName,
                            exceptionString = l.GetExceptionString(),
                            domain = l.Domain,
                            identity = l.Identity
                        };
            if (l.LocationInformation != null)
            {
                var i = l.LocationInformation;
                d.locationInfo = new LogEventLocation
                                     {
                                         className = i.ClassName,
                                         fileName = i.FileName,
                                         lineNumber = i.LineNumber,
                                         methodName = i.MethodName
                                     };
            }

            if (l.Properties != null)
                d.properties = To(l.GetProperties());

            return d;
        }

        public static Dictionary<string, object> To(PropertiesDictionary p)
        {
            var d = new Dictionary<string, object>();
            foreach (var key in p.GetKeys())
            {
                d.Add(key, p[key]);
            }
            return d;
        }

        public static PropertiesDictionary To(Dictionary<string, object> dictionary)
        {
            var dic = new PropertiesDictionary();
            foreach (var item in dictionary)
            {
                dic[item.Key] = item.Value;
            }
            return dic;
        }

        private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    }
}
