using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ElasticElmah.Appender.Web
{
    public class RequestInfo
    {
        public readonly Uri Url;
        public readonly string Method;
        public readonly string Body;
        public RequestInfo(Uri url, string method, string body)
        {
            Url = url;
            Method = method;
            Body = body;
        }
    }
}
