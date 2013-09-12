using System;
using System.Net;
using System.Threading.Tasks;

namespace ElasticElmah.Appender.Web
{
    public static class JsonRequestExtensions
    {
        public static Tuple<HttpStatusCode, string> Sync(this IJSonRequest that, RequestInfo info)
        {
            return that.Sync(info.Url, info.Method, info.Body);
        }
        public static Task<Tuple<HttpStatusCode, string>> Async(this IJSonRequest that, RequestInfo info)
        {
            return that.Async(info.Url, info.Method, info.Body);
        }
        public static T WaitOne<T>(this Tuple<Func<IAsyncResult>, Func<IAsyncResult, T>> that)
        {
            var iar = that.Item1();
            iar.AsyncWaitHandle.WaitOne();
            return that.Item2(iar);
        }
    }
}
