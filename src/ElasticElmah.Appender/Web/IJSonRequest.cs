﻿using System;
using System.Net;

namespace ElasticElmah.Appender.Web
{
    public interface IJSonRequest
    {
        Tuple<HttpStatusCode, string> Sync(Uri uri, string method, string data);
        IAsyncResult Async(Uri uri, string method, string bytes, Action<HttpStatusCode, string> onsuccess);
        Tuple<Func<IAsyncResult>, Func<IAsyncResult, Tuple<HttpStatusCode, string>>> Async(Uri uri, string method, string bytes);
    }
}
