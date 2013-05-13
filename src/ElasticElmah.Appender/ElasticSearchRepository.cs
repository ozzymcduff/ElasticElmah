using System;
using System.Collections.Generic;
using System.Linq;
using ElasticElmah.Appender.Storage;
using Nest;
using log4net.Core;

namespace ElasticElmah.Appender
{
    public class ElasticSearchRepository
    {
        public ElasticSearchRepository(string connectionString)
        {
            var settings = BuildElsticSearchConnection(connectionString);
            _client = new ElasticClient(settings.Item1);
            _index = settings.Item2["Index"];
        }

        public ElasticSearchRepository(ElasticClient client, string index)
        {
            _client = client;
            _index = index;
        }

        public void CreateIndexIfNotExists()
        {
            if (!_client.IndexExists(_index).Exists)
            {
                CreateIndex(_client);
            }
        }

        private readonly ElasticClient _client;
        
        private readonly string _index;


        public IEnumerable<LoggingEventData> All()
        {
            var results = _client.Search<LogEvent>(s => s.Fields("_source"));
            var docs = results.DocumentsWithMetaData.Select(d => Map.To(d.Source));
            return docs;
        }

        public void CreateIndex()
        {
            CreateIndex(_client);
        }
        private void CreateIndex(IElasticClient c)
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
            c.CreateIndex(_index, indexSettings);

        }
        public void DeleteIndex()
        {
            _client.DeleteIndex(_index);
        }

        public void Flush()
        {
            _client.Flush();
        }

        public Tuple<IEnumerable<Tuple<string, LoggingEventData>>, int> GetPaged(int pageIndex, int pageSize)
        {
            var results = _client.Search<LogEvent>(s => s
                .Fields("_source")
                .Skip(pageIndex)
                .Size(pageSize)
                .SortDescending(f=>f.TimeStamp)
                );
            var docs = results.DocumentsWithMetaData.Select(d => new Tuple<string, LoggingEventData>(d.Id, Map.To(d.Source)));
            return new Tuple<IEnumerable<Tuple<string, LoggingEventData>>, int>(docs, results.Total);
        }

        public Tuple<string, LoggingEventData> Get(string id)
        {
            var results = _client.Get<LogEvent>(_index, "LoggingEvent", id);
            if (results == null)
                return null;
            return new Tuple<string, LoggingEventData>(id, Map.To(results));
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
            var c = _client;
            if (c != null)
            {
                if (c.IsValid)
                {
                    var response = c.Index(Map.To(loggingEvent), _index);
                    return response.Id;
                }
            }
            return null;
        }
    }
}
