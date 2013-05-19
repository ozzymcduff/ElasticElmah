using System;
using System.Collections.Generic;
using System.Linq;
using ElasticElmah.Appender.Storage;
using log4net.Core;
using System.Net;
using ElasticElmah.Appender.Web;
using ElasticElmah.Appender.Search;

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
            this.request = request ?? new Web.JsonRequestAsync();
            this.serializer = serializer ?? new DefaultJsonSerializer();
        }

        public void CreateIndexIfNotExists()
        {
            if (!IndexExists())
            {
                CreateIndex(s => { });
            }
        }

        private bool IndexExists()
        {
            var resp = request.Async(UrlToIndex(settings, ""), "HEAD", null)();
            return resp.Item1 == HttpStatusCode.OK;
        }

        private readonly string _index;
        private IDictionary<string, string> settings;

        public IAsyncResult CreateIndex()
        {
            return CreateIndex(s => { });
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
        public IAsyncResult CreateIndex(Action<string> onsuccess)
        {
            var req = CreateIndexRequest();
            return request.Async(req.Url, req.Method, req.Body,
              (code, s) =>
              {
                  onsuccess(s);
              });
        }

        public IAsyncResult DeleteIndex()
        {
            return request.Async(UrlToIndex(settings, ""), "DELETE", null, (c, s) => { });
        }

        /*public void Flush()
        {
            request.Async(Url(settings, "_all/_flush?refresh=false"), "POST", null, (c, s) => { });
        }*/

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
        public IAsyncResult GetPaged(int pageIndex, int pageSize, Action<LogSearchResult> onsuccess)
        {
            return request.Async(GetPagedRequest(pageIndex, pageSize),
              (c, s) =>
              {
                  onsuccess(GetPagedResult(c, s));
              });
        }
        public Func<LogSearchResult> GetPaged(int pageIndex, int pageSize)
        {
            var resp = request.Async(GetPagedRequest(pageIndex, pageSize));
            return () =>
                {
                    var res = resp();
                    return GetPagedResult(res.Item1, res.Item2);
                };
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
        public IAsyncResult Get(string id, Action<LogWithId> success)
        {
            return request.Async(GetRequest(id),
                 (code, s) =>
                 {
                     success(GetGetResponse(s));
                 });
        }

        private LogWithId GetGetResponse(string s)
        {
            var res = serializer.Deserialize<Hit>(s);
            return new LogWithId(res._id, Map.To(res._source));
        }

        public LogWithId Get(string id)
        {
            var resp = request.Async(GetRequest(id));
            return GetGetResponse(resp().Item2);
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
            Add(loggingEvent, (resp) => { });
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
        public IAsyncResult Add(LoggingEvent loggingEvent, Action<string> onsuccess)
        {
            return request.Async(AddRequest(loggingEvent), (code, val) =>
            {
                onsuccess(serializer.Deserialize<AddResponse>(val)._id);
            });
        }
        class Index
        {
            public LogEvent index { get; set; }
        }
        private RequestInfo AddBulkRequest(IEnumerable<LoggingEvent> loggingEvents, bool refresh=false)
        {
            return new RequestInfo(UrlToIndex(settings, "LoggingEvent/_bulk"+(refresh?"?refresh=true":"")), "POST",
                string.Join("\n",loggingEvents
                    .Select(l => serializer.Serialize(
                        new Index { index = Map.To(l) }
                        ).Replace('\r',' ').Replace('\n',' ')))+"\n");
        }
        public IAsyncResult AddBulk(IEnumerable<LoggingEvent> loggingEvents, Action onsuccess,bool refresh=false) 
        {
            return request.Async(AddBulkRequest(loggingEvents, refresh), (code, val) =>
            {
                onsuccess();
            });
        }

        public IAsyncResult Refresh(Action onsuccess=null)
        {
            return request.Async(UrlToIndex(settings, "_refresh" ),"POST", null, (code, val) =>
            {
                if (onsuccess != null) onsuccess();
            });
        }
    }
    public static class Extensions
    {
        public static T Tap<T>(this T that, Action<T> tapaction)
        {
            tapaction(that);
            return that;
        }
    }

}
