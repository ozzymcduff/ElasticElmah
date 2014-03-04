using System;
using System.IO;
using System.Net;
using System.Text;

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
            return new RequestException(HttpStatusCode.InternalServerError, ex.Message);
        }
    }
}
