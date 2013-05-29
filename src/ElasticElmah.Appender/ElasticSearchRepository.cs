using System;
using System.Collections.Generic;
using System.Linq;
using ElasticElmah.Appender.Storage;
using log4net.Core;
using System.Net;
using ElasticElmah.Appender.Web;
using ElasticElmah.Appender.Search;
using System.Threading.Tasks;

namespace ElasticElmah.Appender
{
    public class ElasticSearchRepository
    {
        private readonly IJSonRequest request;
        private readonly IJsonSerializer serializer;
        public ElasticSearchRepository(string connectionString, IJSonRequest request = null, IJsonSerializer serializer = null)
        {
            settings = BuildElsticSearchConnection(connectionString);
            _index = settings["Index"];
            this.request = request ?? new Web.JsonRequest();
            this.serializer = serializer ?? new DefaultJsonSerializer();
        }

        /// <summary>
        /// async request to create index
        /// </summary>
        public Task CreateIndexIfNotExistsAsync()
        {
            return IndexExistsAsync()
                .ContinueWith(t => {
                    var exists = t.Result;
                    if (!exists)
                    {
                        CreateIndexAsync();
                    }
                });
        }

        public void CreateIndexIfNotExists()
        {
            var exists=IndexExists();
            if (!exists)
            {
                CreateIndex();
            }
        }

        private Task<bool> IndexExistsAsync()
        {
            return request.AsTask(IndexExistsRequest(), (t) =>
            {
                return (t.Item1 == HttpStatusCode.OK);
            });
        }

        private RequestInfo IndexExistsRequest()
        {
            return new RequestInfo(UrlToIndex(settings, ""), "HEAD", null);
        }

        private bool IndexExists()
        {
            return request.Sync(IndexExistsRequest()).Item1 == HttpStatusCode.OK;
        }

        private readonly string _index;
        private IDictionary<string, string> settings;

        public void CreateIndex() 
        {
            request.Sync(CreateIndexRequest());
        }
        private RequestInfo CreateIndexRequest()
        {
            return new RequestInfo(UrlToIndex(settings, ""), "POST",
@"{
  ""settings"": {
    ""index"": {
      ""number_of_replicas"": 0,
      ""number_of_shards"": 1
    }
  },
  ""mappings"": {
    ""LoggingEvent"": {
      ""_source"": {
        ""enabled"": true,
        ""compress"": false
      },
      ""_ttl"": {
        ""enabled"": true,
        ""default"": ""24d""
      },
      ""properties"": {
        ""timeStamp"": {
          ""type"": ""date""
        }
      }
    }
  }
}");
        }
        public Task CreateIndexAsync()
        {
            var req = CreateIndexRequest();
            return request.AsTask(req, (t) =>{});
        }

        public Task DeleteIndexAsync()
        {
            return request.AsTask(new RequestInfo(UrlToIndex(settings, ""), "DELETE", null), (t) => { });
        }

        public void DeleteIndex()
        {
            request.Sync(UrlToIndex(settings, ""), "DELETE", null);
        }

        public class SearchResponse
        {
            public class Hits
            {
                public int total { get; set; }
                public Hit[] hits { get; set; }
            }
            public Hits hits { get; set; }
        }

        private RequestInfo GetPagedRequest(int pageIndex, int pageSize)
        {
            return new RequestInfo(UrlToIndex(settings, "LoggingEvent/_search"), "POST",
                  @"{
    ""fields"" : [""_parent"",""_source""],
    ""query""  : {
        ""bool"": {
            ""must"": [],
            ""must_not"": [],
            ""should"": [{""match_all"":{}}]
        }
    },
    ""from"": " + pageIndex + @",
    ""size"": " + pageSize + @",
    ""sort"":[
        {""timeStamp"": {
                ""order"":""desc""
            }
        }
    ],
    ""facets"":{},
    ""version"":true
}");
        }
        public LogSearchResult GetPaged(int pageIndex, int pageSize)
        {
            var res= request.Sync(GetPagedRequest(pageIndex, pageSize));
            return GetPagedResult(res.Item1,res.Item2);
        }
        public Task<LogSearchResult> GetPagedAsync(int pageIndex, int pageSize)
        {
            return request.AsTask(GetPagedRequest(pageIndex, pageSize),
                res => GetPagedResult(res.Item1, res.Item2));
        }
        private LogSearchResult GetPagedResult(HttpStatusCode c, string s)
        {
            var res = serializer.Deserialize<SearchResponse>(s);
            var parsed = new LogSearchResult(res.hits.hits.Select(h => new LogWithId(h._id, Map.To(h._source))), res.hits.total);
            return parsed;
        }

        public class Hit
        {
            public string _id { get; set; }
            public string _type { get; set; }
            public string _index { get; set; }
            public LogEvent _source { get; set; }
        }

        private RequestInfo GetRequest(string id)
        {
            return new RequestInfo(UrlToIndex(settings, "LoggingEvent/" + id), "GET", null);
        }

        private LogWithId GetGetResponse(string s)
        {
            var res = serializer.Deserialize<Hit>(s);
            return new LogWithId(res._id, Map.To(res._source));
        }

        public LogWithId Get(string id)
        {
            return GetGetResponse(request.Sync(GetRequest(id)).Item2);
        }

        public Task<LogWithId> GetAsync(string id)
        {
            return request.AsTask(GetRequest(id), resp=> GetGetResponse(resp.Item2));
        }

        private static IDictionary<string, string> BuildElsticSearchConnection(string connectionString)
        {
            var lookup = connectionString
                .Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Split(new[] { '=' }))
                .ToDictionary(v => v[0], v => v[1], StringComparer.InvariantCultureIgnoreCase);
            var url = Url(lookup);
            return lookup;
        }

        private static Uri Url(IDictionary<string, string> lookup, string t = null)
        {
            var serverAndPort = ServerAndPort(lookup);
            var url = new Uri(serverAndPort + (string.IsNullOrEmpty(t) ? "" : "/" + t));
            return url;
        }

        private static string ServerAndPort(IDictionary<string, string> lookup)
        {
            var server = lookup["Server"];
            var port = lookup["Port"];
            return "http://" + server + ":" + port;
        }

        private static Uri UrlToIndex(IDictionary<string, string> lookup, string t)
        {
            var url = new Uri(ServerAndPort(lookup) + "/" + lookup["Index"] + "/" + t);
            return url;
        }

        public void AddWithoutReturn(LoggingEvent loggingEvent)
        {
            request.Async(AddRequest(loggingEvent), (code, s) => { });
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="loggingEvent"></param>
        /// <returns>the id of the logging event</returns>
        public string Add(LoggingEvent loggingEvent)
        {
            var resp = request.Sync(AddRequest(loggingEvent));
            return serializer.Deserialize<AddResponse>(resp.Item2)._id;
        }
        private class AddResponse
        {
            public bool ok { get; set; }
            public string _index { get; set; }
            public string _type { get; set; }
            public string _id { get; set; }
            public int _version { get; set; }
        }

        private RequestInfo AddRequest(LoggingEvent loggingEvent) 
        {
            return new RequestInfo(UrlToIndex(settings, "LoggingEvent/"), "POST", 
                serializer.Serialize(Map.To(loggingEvent)));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="loggingEvent"></param>
        /// <param name="onsuccess">the id of the item added</param>
        /// <returns></returns>
        public Task<string> AddAsync(LoggingEvent loggingEvent)
        {
            return request.AsTask(AddRequest(loggingEvent), t => serializer.Deserialize<AddResponse>(t.Item2)._id);
        }
        class Index
        {
            public IndexOp index { get; set; }
        }
        class IndexOp
        {
            public string _index { get; set; }
            public string _type { get; set; }
        }
        private RequestInfo AddBulkRequest(IEnumerable<LoggingEvent> loggingEvents, bool refresh=false)
        {
            string operation = serializer.Serialize(new Index { index = new IndexOp() { _index = _index, _type = "LoggingEvent" } });
            return new RequestInfo(UrlToIndex(settings, "LoggingEvent/_bulk" + (refresh ? "?refresh=true" : "")), "POST",
                string.Join(Environment.NewLine, loggingEvents
                    .Select(l => operation + Environment.NewLine 
                        + serializer.Serialize(Map.To(l)))) 
                        + Environment.NewLine
                        );
        }
        public HttpStatusCode AddBulk(IEnumerable<LoggingEvent> loggingEvents, bool refresh=false) 
        {
            return request.Sync(AddBulkRequest(loggingEvents, refresh)).Item1;
        }

        public Task<HttpStatusCode> AddBulkAsync(IEnumerable<LoggingEvent> loggingEvents, bool refresh = false)
        {
            return request.AsTask(AddBulkRequest(loggingEvents, refresh), t=>t.Item1);
        }

        public void Refresh()
        {
            request.Sync(UrlToIndex(settings, "_refresh" ),"POST", null);
        }
    }
    public static class Extensions
    {
        public static T Tap<T>(this T that, Action<T> tapaction)
        {
            tapaction(that);
            return that;
        }
        public static T TapNotNull<T>(this T that, Action<T> tapaction) where T : class
        {
            if (that!=null)
                tapaction(that);
            return that;
        }
    }

}
