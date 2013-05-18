using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace ElasticElmah.Appender.Web
{
    public class Request : IRequest
    {
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
                    //if (response.StatusCode != HttpStatusCode.OK)
                    //{
                    //    throw new RequestException(response.StatusCode, c);
                    //}
                    //else
                    //{
                    return new Tuple<HttpStatusCode,string>(response.StatusCode,c);
                    //}
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

        public void Async(Uri uri, string method, string data,
    Action<HttpStatusCode,string> onsuccess)
        {
            var result = Sync(uri, method, data);
            onsuccess(result.Item1, result.Item2);
        }
        public Func<Tuple<HttpStatusCode,string>> Async(Uri uri, string method, string data)
        {
            var result = Sync(uri, method, data);
            return () =>
            {
                return result;
            };
        }
    }
}
