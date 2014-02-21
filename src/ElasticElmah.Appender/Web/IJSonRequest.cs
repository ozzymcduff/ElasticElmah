using System;
using System.Net;
using System.Threading.Tasks;

namespace ElasticElmah.Appender.Web
{
    public interface IJSonRequest
    {
        Tuple<HttpStatusCode, string> Sync(Uri uri, string method, string data);
#if ASYNC
        Task<Tuple<HttpStatusCode, string>> Async(Uri uri, string method, string bytes);
#endif
    }
}
