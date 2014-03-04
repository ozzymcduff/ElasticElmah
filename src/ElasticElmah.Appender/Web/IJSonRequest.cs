using System;
using System.Net;

namespace ElasticElmah.Appender.Web
{
    public interface IJSonRequest
    {
        Tuple<HttpStatusCode, string> Sync(Uri uri, string method, string data);
    }
}
