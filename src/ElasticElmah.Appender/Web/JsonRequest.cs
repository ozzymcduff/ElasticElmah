using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ElasticElmah.Appender.Web
{
    public class JsonRequest : IJSonRequest
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
                    return new Tuple<HttpStatusCode, string>(response.StatusCode, c);
                }
            }
            catch (WebException ex)
            {
                throw GetRequestException(ex);
            }
        }

        private static Exception GetRequestException(WebException ex) 
        {
            if (ex.Response != null)
            {
                using (var rstream = ex.Response.GetResponseStream())
                using (var reader = new StreamReader(rstream, Encoding.UTF8))
                {
                    var c = reader.ReadToEnd().Replace("\\n","\n").Replace("\\r","\r");
                    var resp = ((HttpWebResponse)ex.Response);
                    if (c.Contains("IndexMissingException"))
                    {
                        return new IndexMissingException();
                    }
                    return new RequestException(resp.StatusCode, c);
                }
            }
            else
            {
                return new RequestException(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public Task<Tuple<HttpStatusCode, string>> Async(Uri uri, string method, string bytes)
        {
            var request = (HttpWebRequest)WebRequest.Create(uri).Tap(r =>
            {
                Request(r, method, bytes);
            });
            return Task.Factory.FromAsync(
                request.BeginGetResponse(null, null),
                (iar) => Response(request, iar));
        }

        private static Tuple<HttpStatusCode, string> Response(HttpWebRequest request, IAsyncResult iar)
        {
            HttpWebResponse response = null;
            try
            {
                response = (HttpWebResponse)request.EndGetResponse(iar);
                using (var rstream = response.GetResponseStream())
                using (var reader = new StreamReader(rstream, Encoding.UTF8))
                {
                    var c = reader.ReadToEnd();
                    return new Tuple<HttpStatusCode, string>(response.StatusCode, c);
                }
            }
            catch (WebException webex) 
            {
                throw GetRequestException(webex);
            }
            finally { if (response != null) response.Close(); }
        }

        private static void Request(WebRequest r, string method, string data)
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
        }
    }
}
