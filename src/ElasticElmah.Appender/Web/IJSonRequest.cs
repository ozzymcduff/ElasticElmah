using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace ElasticElmah.Appender.Web
{
    public interface IJSonRequest
    {
        IAsyncResult Async(Uri uri, string method, string bytes, Action<HttpStatusCode, string> onsuccess);
        Func<Tuple<HttpStatusCode, string>> Async(Uri uri, string method, string bytes);
    }
}
