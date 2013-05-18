using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace ElasticElmah.Appender.Web
{
    public class JsonRequest : IJSonRequest
    {
        class FakeAsyncResult : IAsyncResult
        {
            class FakeWaitHandle : System.Threading.WaitHandle
            {
                private FakeAsyncResult fakeAsyncResult;

                public FakeWaitHandle(FakeAsyncResult fakeAsyncResult)
                {
                    this.fakeAsyncResult = fakeAsyncResult;
                }
                public override bool WaitOne()
                {
                    fakeAsyncResult.IsCompleted = true;
                    return true;
                }
            }
            public object AsyncState
            {
                get { return null; }
            }

            public System.Threading.WaitHandle AsyncWaitHandle
            {
                get { return new FakeWaitHandle(this); }
            }

            public bool CompletedSynchronously
            {
                get { return false; }
            }

            public bool IsCompleted
            {
                get;
                set;
            }
        }

        public Tuple<HttpStatusCode, string> Sync(Uri uri, string method, string data)
        {
            var request = WebRequest.Create(uri).Tap(r =>
            {
                r.Method = method;
                r.ContentType = "application/json; charset=utf-8";
                if (!string.IsNullOrEmpty(data))
                {
                    var bytes = Encoding.UTF8.GetBytes(data);
                    using (var stream = r.GetRequestStream())
                    {
                        stream.Write(bytes, 0, bytes.Length);
                    }
                }
            });
            try
            {
                var response = (HttpWebResponse)request.GetResponse();

                using (var rstream = response.GetResponseStream())
                using (var reader = new StreamReader(rstream, Encoding.UTF8))
                {
                    var c = reader.ReadToEnd();
                    return new Tuple<HttpStatusCode,string>(response.StatusCode,c);
                }
            }
            catch (WebException ex)
            {
                if (ex.Response != null)
                {
                    using (var rstream = ex.Response.GetResponseStream())
                    using (var reader = new StreamReader(rstream, Encoding.UTF8))
                    {
                        var c = reader.ReadToEnd();
                        var resp = ((HttpWebResponse)ex.Response);
                        throw new RequestException(resp.StatusCode, c);
                    }
                }
                else {
                    throw new RequestException(HttpStatusCode.InternalServerError, ex.Message);
                }
            }
        }

        public IAsyncResult Async(Uri uri, string method, string data, Action<HttpStatusCode,string> onsuccess)
        {
            var result = Sync(uri, method, data);
            onsuccess(result.Item1, result.Item2);
            return new FakeAsyncResult();
        }
        public Func<Tuple<HttpStatusCode,string>> Async(Uri uri, string method, string data)
        {
            var result = Sync(uri, method, data);
            Func<Tuple<HttpStatusCode, string>> resp = () =>
            {
                return result;
            };
            return resp;
        }
    }
}
