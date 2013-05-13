using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Nest;
using log4net;
using log4net.Core;

namespace ElasticElmah.Appender
{
    public partial class ElasticSearchRepository
    {
        public ElasticSearchRepository(string connectionString)
        {
            ConnectionString = connectionString;
        }

        private ElasticClient _client;
        private Tuple<ConnectionSettings, IDictionary<string, string>> _settings;
        private static readonly object _lockObj = new object();
        protected virtual ElasticClient Client
        {
            get
            {
                lock (_lockObj)
                {
                    if (_client != null)
                    {
                        return _client;
                    }
                    _settings = BuildElsticSearchConnection(ConnectionString);
                    _client = new ElasticClient(_settings.Item1);
                    if (!_client.IndexExists(Index).Exists)
                    {
                        CreateIndex(_client);
                    }
                    return _client;
                }
            }
        }
        private string Index { get { return _settings.Item2["Index"]; } }

        
        private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private string ConnectionString;

        public IEnumerable<LoggingEventData> All()
        {
            var results = Client.Search<LogEvent>(s => s.Fields("_source"));
            var docs = results.DocumentsWithMetaData.Select(d => Map(d.Source));
            return docs;
        }

        public void CreateIndex()
        {
            CreateIndex(Client);
        }
        private void CreateIndex(ElasticClient c)
        {
            var indexSettings = new IndexSettings();
            indexSettings.NumberOfReplicas = 0;
            indexSettings.NumberOfShards = 1;
            indexSettings.Mappings.Add(new RootObjectMapping()
            {
                TypeNameMarker = "LoggingEvent",
                TtlFieldMapping = new TtlFieldMapping().SetDisabled(false).SetDefault("24d"),
                SourceFieldMapping = new SourceFieldMapping().SetDisabled(false),
                Properties = new Dictionary<string, IElasticType> { { "timeStamp", new DateMapping() } }
            });
            c.CreateIndex(Index, indexSettings);

        }
        public void DeleteIndex()
        {
            Client.DeleteIndex(Index);
        }

        public void Flush()
        {
            Client.Flush();
        }

        public Tuple<IEnumerable<Tuple<string, LoggingEventData>>, int> GetPaged(int pageIndex, int pageSize)
        {
            var results = Client.Search<LogEvent>(s => s.Fields("_source").Skip(pageIndex).Size(pageSize));
            var docs = results.DocumentsWithMetaData.Select(d => new Tuple<string, LoggingEventData>(d.Id, Map(d.Source)));
            return new Tuple<IEnumerable<Tuple<string, LoggingEventData>>, int>(docs, results.Total);
        }

        public Tuple<string, LoggingEventData> Get(string id)
        {
            var results = Client.Get<LogEvent>(Index, "LoggingEvent", id);
            if (results == null)
                return null;
            return new Tuple<string, LoggingEventData>(id, Map(results));
        }

        private static Tuple<ConnectionSettings, IDictionary<string, string>> BuildElsticSearchConnection(string connectionString)
        {
            var lookup = connectionString
                .Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Split(new[] { '=' }))
                .ToDictionary(v => v[0], v => v[1], StringComparer.InvariantCultureIgnoreCase);
            var server = lookup["Server"];
            var port = Convert.ToInt32(lookup["Port"]);
            var settings = new ConnectionSettings(new Uri("http://" + server + ":" + port));
            settings.SetDefaultIndex(lookup["Index"]);
            return new Tuple<ConnectionSettings, IDictionary<string, string>>(settings, lookup);
        }

        public string Add(LoggingEvent loggingEvent)
        {
            var c = Client;
            if (c != null)
            {
                if (c.IsValid)
                {
                    var response = c.Index(Map(loggingEvent), Index);
                    return response.Id;
                }
            }
            return null;
        }
    }
}
