using System.Collections.Generic;
using System.Reflection;
using log4net;
using log4net.Core;
using log4net.Util;

namespace ElasticElmah.Appender.Storage
{
    public class Map
    {
        public static LoggingEventData To(LogEvent l)
        {
            var d = new LoggingEventData
                        {
                            LoggerName = l.LoggerName,
                            Level = _log.Logger.Repository.LevelMap[l.Level],
                            Message = l.Message,
                            ThreadName = l.ThreadName,
                            TimeStamp = l.TimeStamp,
                            UserName = l.UserName,
                            ExceptionString = l.ExceptionString,
                            Domain = l.Domain,
                            Identity = l.Identity,
                        };
            if (l.Properties != null)
                d.Properties = To(l.Properties);

            if (l.LocationInfo != null)
            {
                var i = l.LocationInfo;
                d.LocationInfo = new LocationInfo(i.ClassName, i.MethodName, i.FileName, i.LineNumber);
            }
            return d;
        }

        public static LogEvent To(LoggingEvent l)
        {
            var d = new LogEvent
                        {
                            LoggerName = l.LoggerName,
                            Level = l.Level.Name,
                            Message = l.RenderedMessage,
                            ThreadName = l.ThreadName,
                            TimeStamp = l.TimeStamp,
                            UserName = l.UserName,
                            ExceptionString = l.GetExceptionString(),
                            Domain = l.Domain,
                            Identity = l.Identity
                        };
            if (l.LocationInformation != null)
            {
                var i = l.LocationInformation;
                d.LocationInfo = new LogEventLocation
                                     {
                                         ClassName = i.ClassName,
                                         FileName = i.FileName,
                                         LineNumber = i.LineNumber,
                                         MethodName = i.MethodName
                                     };
            }

            if (l.Properties != null)
                d.Properties = To(l.Properties);

            return d;
        }

        private static Dictionary<string, string> To(PropertiesDictionary p)
        {
            var d = new Dictionary<string, string>();
            foreach (var key in p.GetKeys())
            {
                d.Add(key, p[key].ToString());
            }
            return d;
        }

        private static PropertiesDictionary To(Dictionary<string, string> dictionary)
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
