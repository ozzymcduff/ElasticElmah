using System;
using System.Net;

namespace ElasticElmah.Appender.Web
{
    
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
