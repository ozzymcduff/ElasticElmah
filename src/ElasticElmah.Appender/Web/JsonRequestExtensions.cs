using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace ElasticElmah.Appender.Web
{
    public static class JsonRequestExtensions
    {
        public static Tuple<Func<IAsyncResult>, Func<IAsyncResult, Tuple<HttpStatusCode, string>>> Async(this IJSonRequest that, RequestInfo info) 
        {
            return that.Async(info.Url, info.Method, info.Body);
        }
        public static Tuple<HttpStatusCode, string> Sync(this IJSonRequest that, RequestInfo info)
        {
            return that.Sync(info.Url, info.Method, info.Body);
        }
        public static IAsyncResult Async(this IJSonRequest that, RequestInfo info, Action<HttpStatusCode, string> onsuccess)
        {
            return that.Async(info.Url, info.Method, info.Body, onsuccess);
        }
        public static Tuple<Func<IAsyncResult>, Func<IAsyncResult, T>> Map<T>(this IJSonRequest that, RequestInfo info, Func<Tuple<HttpStatusCode,string>,T> mapfun)
        {
            var resp = that.Async(info);
            return new Tuple<Func<IAsyncResult>, Func<IAsyncResult, T>>(
                () => resp.Item1(),
                (iar) => mapfun(resp.Item2(iar)));
        }
        public static T AwaitOne<T>(this Tuple<Func<IAsyncResult>, Func<IAsyncResult, T>> that)
        {
            var iar = that.Item1();
            iar.AsyncWaitHandle.WaitOne();
            return that.Item2(iar);
        }
    }
}
