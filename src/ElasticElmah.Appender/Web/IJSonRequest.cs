using System;
using System.Net;

namespace ElasticElmah.Appender.Web
{
    public interface IJSonRequest
    {
        JsonResponse Sync(Uri uri, string method, string data);
    }
	public class JsonResponse
	{
		public JsonResponse (HttpStatusCode statusCode, string responseText)
		{
			StatusCode = statusCode;
			ResponseText = responseText;
		}
		public JsonResponse ()
		{
		}
		public HttpStatusCode StatusCode;
		public string ResponseText;
	}
}
