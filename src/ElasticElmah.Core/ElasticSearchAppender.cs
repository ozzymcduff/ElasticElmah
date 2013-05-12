using System;
using System.Linq;
using Nest;
using log4net.Appender;
using log4net.Core;
using System.Collections.Generic;
using log4net;
using System.Reflection;

namespace ElasticElmah.Appender
{
    public class ElasticSearchAppender : AppenderSkeleton
    {
        private ElasticClient _client;
        private Tuple<ConnectionSettings, IDictionary<string, string>> settings;
        protected virtual ElasticClient client
        {
            get
            {
                if (_client != null)
                {
                    return _client;
                }
                settings = ElasticConnectionBuilder.BuildElsticSearchConnection(ConnectionString);
                _client = new ElasticClient(settings.Item1);
                if (!_client.IndexExists(Index).Exists)
                {
                    CreateIndex(_client);
                }
                return _client;
            }
        }
        private string Index { get { return settings.Item2["Index"]; } }
        public string ConnectionString { get; set; }

        /// <summary>
        /// Add a log event to the ElasticSearch Repo
        /// </summary>
        /// <param name="loggingEvent"></param>
        protected override void Append(LoggingEvent loggingEvent)
        {
            var c = client;
            if (c != null)
            {
                if (c.IsValid)
                {
                    c.Index(Map(loggingEvent), Index, "loggingevents");
                }
            }
        }

        public class LogLocationInfo
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
                LocationInfo = new LogLocationInfo();
            }
            public string LoggerName { get; set; }
            public string Level { get; set; }
            public string Message { get; set; }
            public string ThreadName { get; set; }
            public DateTime TimeStamp { get; set; }
            public LogLocationInfo LocationInfo { get; set; }
            public string UserName { get; set; }
            public Dictionary<string, string> Properties { get; set; }
            public string ExceptionString { get; set; }
            public string Domain { get; set; }
            public string Identity { get; set; }
        }

        public static LoggingEventData Map(ElasticSearchAppender.LogEvent l)
        {
            var d = new LoggingEventData();
            d.LoggerName = l.LoggerName;
            d.Level = log.Logger.Repository.LevelMap[l.Level];
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

        private static log4net.Util.PropertiesDictionary Map(Dictionary<string, string> dictionary)
        {
            var dic = new log4net.Util.PropertiesDictionary();
            foreach (var item in dictionary)
            {
                dic[item.Key] = item.Value;
            }
            return dic;
        }

        private object Map(LoggingEvent l)
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
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private Dictionary<string, string> Map(log4net.Util.PropertiesDictionary p)
        {
            var d = new Dictionary<string, string>();
            foreach (var key in p.GetKeys())
            {
                d.Add(key, p[key].ToString());
            }
            return d;
        }
        public IEnumerable<log4net.Core.LoggingEventData> All()
        {
            var results = client.Search<ElasticSearchAppender.LogEvent>(s => s.Type("loggingevents").Fields("_source"));
            var docs = results.DocumentsWithMetaData.Select(d => Map(d.Source));
            return docs;
        }

        public void CreateIndex()
        {
            CreateIndex(client);
        }
        private void CreateIndex(ElasticClient c)
        {
            var indexSettings = new IndexSettings();
            indexSettings.NumberOfReplicas = 0;
            indexSettings.NumberOfShards = 1;
            indexSettings.Mappings.Add(new RootObjectMapping()
            {
                TypeNameMarker = "loggingevents",
                TtlFieldMapping = new TtlFieldMapping().SetDisabled(false).SetDefault("24d"),
                SourceFieldMapping = new SourceFieldMapping().SetDisabled(false),
                Properties = new Dictionary<string, IElasticType> { { "timeStamp", new DateMapping() } }
            });
            c.CreateIndex(Index, indexSettings);

        }
        public void DeleteIndex()
        {
            client.DeleteIndex(Index);
        }

        public void Flush()
        {
            client.Flush();
        }


        public Tuple<IEnumerable<Tuple<string, LoggingEventData>>, int> GetPaged(int pageIndex, int pageSize)
        {
            var results = client.Search<LogEvent>(s => s.Type("loggingevents").Fields("_source").Skip(pageIndex).Size(pageSize));
            var docs = results.DocumentsWithMetaData.Select(d => new Tuple<string, LoggingEventData>(d.Id, Map(d.Source)));
            return new Tuple<IEnumerable<Tuple<string, LoggingEventData>>, int>(docs, results.Total);
        }

        public Tuple<string, LoggingEventData> Get(string id)
        {
            var results = client.Get<LogEvent>(Index, "loggingevents", id);
            if (results == null)
                return null;
            return new Tuple<string, LoggingEventData>(id, Map(results));
        }
    }
}
