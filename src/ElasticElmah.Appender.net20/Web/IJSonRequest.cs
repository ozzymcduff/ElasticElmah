using System;
using System.Net;

namespace ElasticElmah.Appender.Web
{
    public interface IJSonRequest
    {
		JsonResponse Request(RequestInfo request);

		JsonResponse Request(Uri uri, string method, string data);
    }
}
