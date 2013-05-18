using System;
using System.Collections.Generic;
using System.Linq;
using ElasticElmah.Appender.Storage;
using log4net.Core;
using System.Net;
using ElasticElmah.Appender.Web;

namespace ElasticElmah.Appender
{
    public class ElasticSearchRepository
    {
        private readonly IRequest request;

        public ElasticSearchRepository(string connectionString, IRequest request = null)
        {
            settings = BuildElsticSearchConnection(connectionString);
            _index = settings["Index"];
            this.request = request ?? new Web.RequestAsync();
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
            return resp.Item1==HttpStatusCode.OK;
        }

        private readonly string _index;
        private IDictionary<string, string> settings;

        public void CreateIndex()
        {
            CreateIndex(s => { });
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
        public void CreateIndex(Action<string> onsuccess)
        {
            var req = CreateIndexRequest();
            request.Async(req.Url, req.Method, req.Body,
              (code,s) =>
              {
                  onsuccess(s);
              });
        }

        public void DeleteIndex()
        {
            request.Async(UrlToIndex(settings,""), "DELETE", null, (c, s) => { });
        }

        public void Flush()
        {
            request.Async(Url(settings, "_all/_flush?refresh=false"), "POST", null, (c, s) => { });
        }

        public class Errors
        {
            public readonly IEnumerable<Error> Documents;
            public readonly int Total;

            public Errors(IEnumerable<Error> docs, int count)
            {
                this.Documents = docs;
                this.Total = count;
            }

        }
        public class Error
        {
            public readonly string Id;
            public readonly LoggingEventData Data;

            public Error(string id, LoggingEventData data)
            {
                this.Id = id;
                this.Data = data;
            }
        }

        public class SearchResponse
        {
            public class Hits
            {
                public int total { get; set; }
                public double? max_score { get; set; }
                public Hit[] hits { get; set; }
            }
            public class Hit
            {
                public string _id { get; set; }
                public string _type { get; set; }
                public string _index { get; set; }
                public int? _version { get; set; }
                public Source _source { get; set; }
                public double? _score { get; set; }
            }
            public class Source
            {
                public LogEvent LoggingEvent { get; set; }
            }
            public int? took { get; set; }
            public bool timed_out { get; set; }
            public Hits hits { get; set; }
        }

        class RequestInfo
        {
            public Uri Url;
            public string Method;
            public string Body;
            public RequestInfo(Uri url, string method, string body)
            {
                Url = url;
                Method = method;
                Body = body;
            }
        }
        private RequestInfo GetPagedRequest(int pageIndex, int pageSize)
        {
            return new RequestInfo(UrlToIndex(settings, "LoggingEvent/_search"), "POST",
                  @"{
""fields"":[""_parent"",""_source""],
""query"":{""bool"":{""must"":[],""must_not"":[],""should"":[{""match_all"":{}}]}},
""from"":" + pageIndex + @",""size"":" + pageSize + @",
""sort"":[{""timeStamp"":{""order"":""desc""}}],
""facets"":{},""version"":true}");
        }
        public void GetPaged(int pageIndex, int pageSize, Action<Errors> onsuccess)
        {
            var req = GetPagedRequest(pageIndex, pageSize);
            request.Async(req.Url, req.Method, req.Body,
              (c,s)=>
              {
                  onsuccess(GetPagedResult(c,s));
              });
        }
        public Func<Errors> GetPaged(int pageIndex, int pageSize)
        {
            var req = GetPagedRequest(pageIndex, pageSize);
            var resp = request.Async(req.Url, req.Method, req.Body);
            return () =>
                {
                    var res = resp();
                    return GetPagedResult(res.Item1,res.Item2);
                };
        }
        private static Errors GetPagedResult(HttpStatusCode c,string s)
        {
            var res = Newtonsoft.Json.JsonConvert.DeserializeObject<SearchResponse>(s);
            //var res = new System.Web.Script.Serialization.JavaScriptSerializer()
            //  .Deserialize<SearchResponse>(s);
            var parsed = new Errors(res.hits.hits.Select(h => new Error(h._id, Map.To(h._source.LoggingEvent))), res.hits.total);
            return parsed;
        }

        public class GetResponse
        {
            public string _id { get; set; }
            public string _type { get; set; }
            public string _index { get; set; }
            public int? _version { get; set; }
            public LogEvent _source { get; set; }
        }

        private RequestInfo GetRequest(string id)
        {
            return new RequestInfo(UrlToIndex(settings, "LoggingEvent/" + id), "GET", null);
        }
        public void Get(string id, Action<Error> success)
        {
            var req = GetRequest(id);
            request.Async(req.Url, req.Method, req.Body,
                 (code,s) =>
                 {
                     success(GetGetResponse(s));
                 });
        }

        private static Error GetGetResponse(string s)
        {
            var res = new System.Web.Script.Serialization.JavaScriptSerializer()
              .Deserialize<GetResponse>(s);
            var resp = new Error(res._id, Map.To(res._source));
            return resp;
        }

        public Error Get(string id)
        {
            var req = GetRequest(id);
            var resp = request.Async(req.Url, req.Method, req.Body);
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

        private static Uri Url(IDictionary<string, string> lookup, string t=null)
        {
            var serverAndPort = ServerAndPort(lookup);
            var url = new Uri(serverAndPort+(string.IsNullOrEmpty(t)?"":"/"+t));
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
        public void Add(LoggingEvent loggingEvent)
        {
            Add(loggingEvent, (c,s) => { });
        }
        public class Index
        {
            public LogEvent LoggingEvent { get; private set; }

            public Index(LogEvent logEvent)
            {
                this.LoggingEvent = logEvent;
            }

        }
        public void Add(LoggingEvent loggingEvent, Action<HttpStatusCode,string> onsuccess)
        {
            var data = new System.Web.Script.Serialization.JavaScriptSerializer()
                .Serialize(new Index(Map.To(loggingEvent)));

            request.Async(UrlToIndex(settings, "LoggingEvent/"), "POST", data, onsuccess);
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
