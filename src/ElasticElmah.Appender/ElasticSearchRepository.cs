using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
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
        private readonly IJSonRequest _request;
        private readonly IJsonSerializer _serializer;

        public ElasticSearchRepository(string connectionString, IJSonRequest request = null, IJsonSerializer serializer = null)
        {
            settings = BuildElsticSearchConnection(connectionString);
            _index = settings["Index"];
            _request = request ?? new JsonRequest();
            _serializer = serializer ?? new DefaultJsonSerializer();
        }

        /// <summary>
        /// async request to create index
        /// </summary>
        public Task CreateIndexOrRefreshMappingsAsync()
        {
            return IndexExistsAsync()
                .ContinueWith(t => { t.PropagateExceptions(); return !t.Result ? CreateIndexAsync() : PutMappingAsync(); });
        }

        public void CreateIndexOrRefreshMappings()
        {
            var exists = IndexExists();
            if (!exists)
            {
                CreateIndex();
            }else
            {
                PutMapping();
            }
        }

        private Task<bool> IndexExistsAsync()
        {
            return _request.Async(IndexExistsRequest()).ContinueWith(t =>
            {
                if (t.Exception != null)
                {
                    if (t.Exception.InnerExceptions.Any(
                            ex => ex is RequestException &&
                                ((RequestException) ex).HttpStatusCode == HttpStatusCode.NotFound))
                        return false;
                }
                return (t.Result.Item1 == HttpStatusCode.OK);
            });
        }

        private RequestInfo IndexExistsRequest()
        {
            return new RequestInfo(UrlToIndex(settings, ""), "HEAD", null);
        }

        private bool IndexExists()
        {
            try
            {
                return _request.Sync(IndexExistsRequest()).Item1 == HttpStatusCode.OK;
            }
            catch (RequestException reqException)
            {
                if (reqException.HttpStatusCode == HttpStatusCode.NotFound)
                    return false;
                throw;
            }
        }

        private readonly string _index;
        private IDictionary<string, string> settings;

        public Task PutMappingAsync()
        {
            var req = PutMappingRequestInfo();
            return _request.Async(req);
        }
        public void PutMapping()
        {
            _request.Sync(PutMappingRequestInfo());
        }

        public void CreateIndex()
        {
            _request.Sync(CreateIndexRequest());
        }

        public Task CreateIndexAsync()
        {
            var req = CreateIndexRequest();
            return _request.Async(req);
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
    ""LoggingEvent"": " + LoggingEventMappings() + @"
  }
}");
        }
        private RequestInfo PutMappingRequestInfo()
        {
            return new RequestInfo(UrlToIndex(settings, "LoggingEvent/_mapping"), "PUT",
@"{
  ""mappings"": {
    ""LoggingEvent"": " + LoggingEventMappings() + @"
  }
}");
        }

        private static string LoggingEventMappings()
        {
            return @"{
      ""_source"": {
        ""enabled"": true,
        ""compress"": false
      },
      ""_ttl"": {
        ""enabled"": true,
        ""default"": ""24d""
      },
      ""_timestamp"" : {
        ""enabled"" : true,
        ""path"" : ""timeStamp"",
        ""store"" : true
      },
      ""properties"": {
        ""timeStamp"": {
          ""type"": ""date""
        },
        ""message"":{""type"" : ""string""},
        ""exceptionString"":{""type"" : ""string""},
        ""domain"":{""type"" : ""string""},
        ""identity"":{""type"" : ""string""},
        ""userName"": {""type"" : ""string""},
        ""locationInfo"":{
            ""type"" : ""object"",
            ""properties"" : {
                ""className"":{""type"" : ""string""},
                ""fileName"":{""type"" : ""string""},
                ""lineNumber"":{""type"" : ""string""},
                ""methodName"":{""type"" : ""string""}
            }
        },
        ""threadName"":{""type"" : ""string""},
        ""loggerName"":{""type"" : ""string""},
        ""level"":{""type"":""string""},
        ""level"":{""type"":""string""},
        ""properties"":{ ""type"" : ""object"", ""store"" : ""yes"" }
      }
    }";
        }

        public Task DeleteIndexAsync()
        {
            return _request.Async(new RequestInfo(UrlToIndex(settings, ""), "DELETE", null));
        }

        public void DeleteIndex()
        {
            _request.Sync(UrlToIndex(settings, ""), "DELETE", null);
        }

        public class SearchResponse
        {
            public class Hits
            {
                public int total { get; set; }
                public Hit[] hits { get; set; }
            }
            public Hits hits { get; set; }
            public Dictionary<string, FacetResult> facets { get; set; }
        }

        public class FacetResult 
        {
            public int? count { get; set; }
            public string _type { get; set; }// query, date_histogram
            public FacetEntry[] entries { get; set; }
        }
        public class FacetEntry
        {
            public int count { get; set; }
            public long time { get; set; }
            public DateTime GetTime()
            {
                var epoch = new DateTime(1970, 1, 1, 0, 0, 0, 0);
                //var offset= TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now);
                return epoch.AddMilliseconds(time);//.Add(offset);
            }
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


        public LogSearchHistogramResult GetTimestampHistogram(string query, DateTime @from, DateTime @to, int pageIndex, int pageSize)
        {
            var res = _request.Sync(GetTimestampHistogramRequest(query, @from, @to, pageIndex, pageSize));
            return GetLogSearchHistogramResultResult(res.Item1, res.Item2);
        }
        public Task<LogSearchHistogramResult> GetTimestampHistogramAsync(string query, DateTime @from, DateTime @to, int pageIndex, int pageSize)
        {
            return _request.Async(GetTimestampHistogramRequest(query, @from, @to, pageIndex, pageSize))
                .ContinueWith(res => { res.PropagateExceptions(); return GetLogSearchHistogramResultResult(res.Result.Item1, res.Result.Item2); });
        }

        private RequestInfo GetTimestampHistogramRequest(string query, DateTime @from, DateTime @to, int pageIndex, int pageSize)
        {
            if (string.IsNullOrWhiteSpace(query))
                query = "*";
            return new RequestInfo(UrlToIndex(settings, "LoggingEvent/_search"), "POST",
                  @"{
    ""from"": " + pageIndex + @",
    ""size"": " + pageSize + @",
    ""facets"": {
        ""facetchart"": {
          ""date_histogram"": {
            ""field"": ""timeStamp"",
            ""interval"": ""5m""
          },
          ""facet_filter"": {
            ""fquery"": {
              ""query"": {
                ""filtered"": {
                  ""query"": {
                    ""query_string"": {
                      ""query"": """ + query + @"""
                    }
                  },
                  ""filter"": {
                    ""range"": {
                      ""timeStamp"": {
                        ""from"": """ + FormatTime(@from) + @""",
                        ""to"": """ + FormatTime(@to) + @"""
                      }
                    }
                  }
                }
              }
            }
          }
        }
      },
    ""version"":true
}");
        }

        public static string FormatTime(DateTime time) 
        {
            return time.ToString("yyyy-MM-ddTHH:mm:ss.fffffffzzz");
        }

        public LogSearchFacetResult GetTimestampFacet(string query, DateTime @from, DateTime @to, int pageIndex, int pageSize)
        {
            var res = _request.Sync(GetTimestampFacetsRequest(query, @from, @to, pageIndex, pageSize));
            return GetFacetResult(res.Item1, res.Item2);
        }
        public Task<LogSearchFacetResult> GetTimestampFacetAsync(string query, DateTime @from, DateTime @to, int pageIndex, int pageSize)
        {
            return _request.Async(GetTimestampFacetsRequest(query, @from, @to, pageIndex, pageSize))
                .ContinueWith(res => { res.PropagateExceptions(); return GetFacetResult(res.Result.Item1, res.Result.Item2); });
        }

        private RequestInfo GetTimestampFacetsRequest(string query, DateTime @from, DateTime @to, int pageIndex, int pageSize)
        {
            if (string.IsNullOrWhiteSpace(query))
                query = "*";
            return new RequestInfo(UrlToIndex(settings, "LoggingEvent/_search"), "POST",
                  @"{
    ""from"": " + pageIndex + @",
    ""size"": " + pageSize + @",
      ""facets"": {
        ""facetquery0"": {
          ""query"": {
            ""filtered"": {
              ""query"": {
                ""query_string"": {
                  ""query"": """ + query + @"""
                }
              },
              ""filter"": {
                ""range"": {
                  ""timeStamp"": {
                    ""from"": """ + FormatTime(@from) + @""",
                    ""to"": """ + FormatTime(@to) + @"""
                  }
                }
              }
            }
          }
        }
      },

    ""version"":true
}");
        }

        public LogSearchResult GetTimestampRange(string query, DateTime @from, DateTime @to, int pageIndex, int pageSize)
        {
            var res = _request.Sync(GetTimestampRangeRequest(query, @from, @to, pageIndex, pageSize));
            return GetPagedResult(res.Item1, res.Item2);
        }
        public Task<LogSearchResult> GetTimestampRangeAsync(string query, DateTime @from, DateTime @to, int pageIndex, int pageSize)
        {
            return _request.Async(GetTimestampRangeRequest(query, @from, @to, pageIndex, pageSize))
                .ContinueWith(res => { res.PropagateExceptions(); return GetPagedResult(res.Result.Item1, res.Result.Item2); });
        }
        private RequestInfo GetTimestampRangeRequest(string query, DateTime @from, DateTime @to, int pageIndex, int pageSize)
        {
            if (string.IsNullOrWhiteSpace(query))
                query = "*";
            return new RequestInfo(UrlToIndex(settings, "LoggingEvent/_search"), "POST",
                  @"{
    ""fields"" : [""_parent"",""_source""],
      ""query"": {
    ""filtered"": {
      ""query"": {
        ""query_string"": {
          ""query"": """+query+@"""
        }
      },
      ""filter"": {
        ""range"": {
          ""timeStamp"": {
            ""from"": """ + FormatTime(@from)+ @""",
            ""to"": """ + FormatTime(@to) + @"""
          }
        }
      }
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
            try
            {
                var res = _request.Sync(GetPagedRequest(pageIndex, pageSize));
                return GetPagedResult(res.Item1, res.Item2);
            }
            catch (RequestException ex)
            {
                if (ex.Message.Contains("IndexMissingException"))
                {
                    throw new IndexMissingException("Missing index!", ex);
                }
                throw;
            }
        }
        public Task<LogSearchResult> GetPagedAsync(int pageIndex, int pageSize)
        {
            return _request.Async(GetPagedRequest(pageIndex, pageSize))
                .ContinueWith(res =>
                                  {
                                      res.PropagateExceptions();
                                      return GetPagedResult(res.Result.Item1, res.Result.Item2);
                                  });
        }
        private LogSearchResult GetPagedResult(HttpStatusCode c, string s)
        {
            var res = _serializer.Deserialize<SearchResponse>(s);
            var parsed = new LogSearchResult(res.hits.hits.Select(h => new LogWithId(h._id, Map.To(h._source))), res.hits.total);
            return parsed;
        }
        private LogSearchFacetResult GetFacetResult(HttpStatusCode c, string s) 
        {
            var res = _serializer.Deserialize<SearchResponse>(s);
            var parsed = new LogSearchFacetResult();
            parsed.Count = res.facets.Single().Value.count.Value;
            return parsed;
        }
        private LogSearchHistogramResult GetLogSearchHistogramResultResult(HttpStatusCode c, string s)
        {
            var res = _serializer.Deserialize<SearchResponse>(s);
            var parsed = new LogSearchHistogramResult();
            parsed.Histogram = res.facets.Single().Value.entries.Select(e => 
                new HistogramEntry { 
                    Count=e.count, 
                    Time=e.GetTime() 
                }).ToArray();
            return parsed;
        }
        public LogSearchResult GetPaged(SearchTerm search, int pageIndex, int pageSize)
        {
            var res = _request.Sync(GetPagedRequest(search, pageIndex, pageSize));
            return GetPagedResult(res.Item1, res.Item2);
        }
        public Task<LogSearchResult> GetPagedAsync(SearchTerm search, int pageIndex, int pageSize)
        {
            return _request.Async(GetPagedRequest(search, pageIndex, pageSize))
                .ContinueWith(res => { res.PropagateExceptions(); return GetPagedResult(res.Result.Item1, res.Result.Item2); });
        }

        public class SearchTerm
        {
            public string PropertyName { get; set; }
            public string Value { get; set; }
        }

        private RequestInfo GetPagedRequest(SearchTerm search, int pageIndex, int pageSize)
        {
            return new RequestInfo(UrlToIndex(settings, "LoggingEvent/_search"), "POST",
                  @"{
    ""fields"" : [""_parent"",""_source""],
    ""query""  : {
        ""bool"": {
            ""must"":[{""term"":{""" + search.PropertyName + @""":""" + search.Value + @"""}}],
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
            var res = _serializer.Deserialize<Hit>(s);
            return new LogWithId(res._id, Map.To(res._source));
        }

        public LogWithId Get(string id)
        {
            return GetGetResponse(_request.Sync(GetRequest(id)).Item2);
        }

        public Task<LogWithId> GetAsync(string id)
        {
            return _request.Async(GetRequest(id))
                .ContinueWith(resp => { resp.PropagateExceptions(); return GetGetResponse(resp.Result.Item2); });
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
            _request.Async(AddRequest(loggingEvent));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="loggingEvent"></param>
        /// <returns>the id of the logging event</returns>
        public string Add(LoggingEvent loggingEvent)
        {
            var resp = _request.Sync(AddRequest(loggingEvent));
            return _serializer.Deserialize<AddResponse>(resp.Item2)._id;
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
                _serializer.Serialize(Map.To(loggingEvent)));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="loggingEvent"></param>
        /// <param name="onsuccess">the id of the item added</param>
        /// <returns></returns>
        public Task<string> AddAsync(LoggingEvent loggingEvent)
        {
            return _request.Async(AddRequest(loggingEvent))
                .ContinueWith(t => { t.PropagateExceptions(); return _serializer.Deserialize<AddResponse>(t.Result.Item2)._id; });
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
        private RequestInfo AddBulkRequest(IEnumerable<LoggingEvent> loggingEvents, bool refresh = false)
        {
            string operation = _serializer.Serialize(new Index { index = new IndexOp() { _index = _index, _type = "LoggingEvent" } });
            return new RequestInfo(UrlToIndex(settings, "LoggingEvent/_bulk" + (refresh ? "?refresh=true" : "")), "POST",
                string.Join(Environment.NewLine, loggingEvents
                    .Select(l => operation + Environment.NewLine
                        + _serializer.Serialize(Map.To(l))))
                        + Environment.NewLine
                        );
        }
        public HttpStatusCode AddBulk(IEnumerable<LoggingEvent> loggingEvents, bool refresh = false)
        {
            return _request.Sync(AddBulkRequest(loggingEvents, refresh)).Item1;
        }

        public Task<HttpStatusCode> AddBulkAsync(IEnumerable<LoggingEvent> loggingEvents, bool refresh = false)
        {
            return _request.Async(AddBulkRequest(loggingEvents, refresh))
                .ContinueWith(t => { t.PropagateExceptions(); return t.Result.Item1; });
        }

        public void Refresh()
        {
            _request.Sync(UrlToIndex(settings, "_refresh"), "POST", null);
        }
    }

    public static class AggregateExceptionExtensions
    {
        public static IEnumerable<Exception> UnWrapInnerExceptions(this AggregateException exception)
        {
            var exception2 = exception;
            while ((exception2 = exception2.InnerException as AggregateException) != null)
            {
                exception = exception2;
            }
            return exception.InnerExceptions;
        }
    }

    internal static class Extensions
    {
        public static T Tap<T>(this T that, Action<T> tapaction)
        {
            tapaction(that);
            return that;
        }
        public static T TapNotNull<T>(this T that, Action<T> tapaction) where T : class
        {
            if (that != null)
                tapaction(that);
            return that;
        }
        public static void PropagateExceptions(this Task task)
        {
            if (task == null)
                throw new ArgumentNullException("task");
            if (!task.IsCompleted)
                throw new InvalidOperationException("The task has not completed yet.");

            if (task.IsFaulted)
                task.Wait();
        }
    }

    [Serializable]
    public class IndexMissingException : Exception
    {
        public IndexMissingException()
        {
        }

        public IndexMissingException(string message) : base(message)
        {
        }

        public IndexMissingException(string message, Exception inner) : base(message, inner)
        {
        }

        protected IndexMissingException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}
