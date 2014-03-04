using System;
using System.Net;

namespace ElasticElmah.Appender.Web
{
    public static class JsonRequestExtensions
    {
        public static Tuple<HttpStatusCode, string> Sync(this IJSonRequest that, RequestInfo info)
        {
            return that.Sync(info.Url, info.Method, info.Body);
        }
    }
}
