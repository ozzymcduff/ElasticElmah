using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace ElasticElmah.Appender.Web
{
    public static class JsonRequestExtensions
    {
        public static Func<Tuple<HttpStatusCode, string>> Async(this IJSonRequest that, RequestInfo info) 
        {
            return that.Async(info.Url, info.Method, info.Body);
        }
        public static IAsyncResult Async(this IJSonRequest that, RequestInfo info, Action<HttpStatusCode, string> onsuccess)
        {
            return that.Async(info.Url, info.Method, info.Body, onsuccess);
        }
    }
}
