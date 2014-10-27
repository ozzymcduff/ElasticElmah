using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using ElasticElmah.Appender.Storage;
using System.Net;
using ElasticElmah.Appender.Web;
using ElasticElmah.Appender.Search;
using LoggingEvent = ElasticElmah.Appender.Storage.LoggingEvent;

namespace ElasticElmah.Appender
{
    public class ElasticSearchRepository
    {
        private readonly IJSonRequest _request;
        private readonly IJsonSerializer _serializer;

        public ElasticSearchRepository(string connectionString, IJSonRequest request = null, IJsonSerializer serializer = null)
        {
            _settings = BuildElsticSearchConnection(connectionString);
            _index = _settings["Index"];
            _request = request ?? new JsonRequest();
            _serializer = serializer?? new DefaultJsonSerializer();
        }
        public void CreateIndexOrRefreshMappings()
        {
            var exists = IndexExists();
            if (!exists)
            {
                CreateIndex();
            }
            else
            {
                PutMapping();
            }
        }
        private RequestInfo IndexExistsRequest()
        {
            return new RequestInfo(UrlToIndex(_settings, ""), "HEAD", null);
        }

        private bool IndexExists()
        {
            try
            {
				return _request.Request(IndexExistsRequest()).StatusCode == HttpStatusCode.OK;
            }
            catch (RequestException reqException)
            {
                if (reqException.HttpStatusCode == HttpStatusCode.NotFound)
                    return false;
                throw;
            }
        }

        private readonly string _index;
        private readonly IDictionary<string, string> _settings;
        public void PutMapping()
        {
            _request.Request(PutMappingRequestInfo());
        }

        public void CreateIndex()
        {
            _request.Request(CreateIndexRequest());
        }
        private RequestInfo CreateIndexRequest()
        {
            return new RequestInfo(UrlToIndex(_settings, ""), "POST",
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
            return new RequestInfo(UrlToIndex(_settings, "LoggingEvent/_mapping"), "PUT",
@"{
    ""LoggingEvent"": " + LoggingEventMappings() + @"
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
        ""properties"":{ ""type"" : ""object"", ""store"" : ""yes"" }
      }
    }";
        }
        public void DeleteIndex()
        {
            try
            {
                _request.Request(UrlToIndex(_settings, ""), "DELETE", null);
            }
            catch (IndexMissingException) { }
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
            return new RequestInfo(UrlToIndex(_settings, "LoggingEvent/_search"), "POST",
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


        public static string FormatTime(DateTime time)
        {
            return Map.To(time);
        }

        public LogSearchResult GetTimestampRange(string query, DateTime @from, DateTime @to, int pageIndex, int pageSize)
        {
            var res = _request.Request(GetTimestampRangeRequest(query, @from, @to, pageIndex, pageSize));
			return GetPagedResult(res.ResponseText);
        }

        private RequestInfo GetTimestampRangeRequest(string query, DateTime @from, DateTime @to, int pageIndex, int pageSize)
        {
            if (string.IsNullOrEmpty(query))
                query = "*";
            return new RequestInfo(UrlToIndex(_settings, "LoggingEvent/_search"), "POST",
                  @"{
    ""fields"" : [""_parent"",""_source""],
      ""query"": {
    ""filtered"": {
      ""query"": {
        ""query_string"": {
          ""query"": " + Encode(query) + @"
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

        private string Encode(string query)
        {
            return _serializer.Serialize(query);
        }

        public LogSearchResult GetPaged(int pageIndex, int pageSize)
        {
            try
            {
                var res = _request.Request(GetPagedRequest(pageIndex, pageSize));
				return GetPagedResult(res.ResponseText);
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

        private LogSearchResult GetPagedResult(string s)
        {
            var res = _serializer.Deserialize<SearchResponse>(s);
			var list = new List<LogWithId> ();
			foreach (var hit in res.hits.hits) {
				list.Add(new LogWithId(hit._id, Map.To(hit._source)));
			}
            var parsed = new LogSearchResult(list, res.hits.total);
            return parsed;
        }
        public LogSearchResult GetPaged(SearchTerm search, int pageIndex, int pageSize)
        {
            var res = _request.Request(GetPagedRequest(search, pageIndex, pageSize));
			return GetPagedResult(res.ResponseText);
        }

        public class SearchTerm
        {
            public string PropertyName { get; set; }
            public string Value { get; set; }
        }

        private RequestInfo GetPagedRequest(SearchTerm search, int pageIndex, int pageSize)
        {
            return new RequestInfo(UrlToIndex(_settings, "LoggingEvent/_search"), "POST",
                  @"{
    ""fields"" : [""_parent"",""_source""],
    ""query""  : {
        ""bool"": {
            ""must"":[{""term"":{" + Encode(search.PropertyName) + @":" + Encode(search.Value) + @"}}],
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
            public LoggingEvent _source { get; set; }
        }

        private RequestInfo GetRequest(string id)
        {
            return new RequestInfo(UrlToIndex(_settings, "LoggingEvent/" + id), "GET", null);
        }

        private LogWithId GetGetResponse(string s)
        {
            var res = _serializer.Deserialize<Hit>(s);
            return new LogWithId(res._id, Map.To(res._source));
        }

        public LogWithId Get(string id)
        {
			return GetGetResponse(_request.Request(GetRequest(id)).ResponseText);
        }

        private static IDictionary<string, string> BuildElsticSearchConnection(string connectionString)
        {
			var builder = new System.Data.Common.DbConnectionStringBuilder();
			builder.ConnectionString = connectionString.Replace("{", "\"").Replace("}", "\"");
			var lookup = new Dictionary<string,string>(StringComparer.InvariantCultureIgnoreCase);
			foreach (string key in builder.Keys)
			{
				lookup[key] = Convert.ToString(builder[key]);
			}
            return lookup;
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="loggingEvent"></param>
        /// <returns>the id of the logging event</returns>
        public string Add(log4net.Core.LoggingEvent loggingEvent)
        {
            var resp = _request.Request(AddRequest(loggingEvent));
			return _serializer.Deserialize<AddResponse>(resp.ResponseText)._id;
        }
        private class AddResponse
        {
            public bool ok { get; set; }
            public string _index { get; set; }
            public string _type { get; set; }
            public string _id { get; set; }
            public int _version { get; set; }
        }

        private RequestInfo AddRequest(log4net.Core.LoggingEvent loggingEvent)
        {
            return new RequestInfo(UrlToIndex(_settings, "LoggingEvent/"), "POST",
                _serializer.Serialize(Map.To(loggingEvent)));
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
        private RequestInfo AddBulkRequest(IEnumerable<log4net.Core.LoggingEvent> loggingEvents, bool refresh = false)
        {
            string operation = _serializer.Serialize(new Index { index = new IndexOp() { _index = _index, _type = "LoggingEvent" } });
            return new RequestInfo(UrlToIndex(_settings, "LoggingEvent/_bulk" + (refresh ? "?refresh=true" : "")), "POST",
				string.Join(Environment.NewLine, SerializeForBulk(operation,loggingEvents))
                        + Environment.NewLine
                        );
        }
		private string[] SerializeForBulk(string operation, IEnumerable<log4net.Core.LoggingEvent> loggingEvents){
			var sb = new List<string> ();
			foreach (var l in loggingEvents) {
				sb.Add (operation);

				sb.Add(_serializer.Serialize(Map.To(l)));
			}
			return sb.ToArray ();
		}
        public HttpStatusCode AddBulk(IEnumerable<log4net.Core.LoggingEvent> loggingEvents, bool refresh = false)
        {
			return _request.Request(AddBulkRequest(loggingEvents, refresh)).StatusCode;
        }

        public void Refresh()
        {
            _request.Request(UrlToIndex(_settings, "_refresh"), "POST", null);
        }
    }

    [Serializable]
    public class IndexMissingException : Exception
    {
        public IndexMissingException()
        {
        }

        public IndexMissingException(string message)
            : base(message)
        {
        }

        public IndexMissingException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected IndexMissingException(
            SerializationInfo info,
            StreamingContext context)
            : base(info, context)
        {
        }
    }
}
