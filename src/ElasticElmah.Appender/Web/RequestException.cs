using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ElasticElmah.Appender.Web
{
    public class RequestException : Exception
    {
        public readonly System.Net.HttpStatusCode HttpStatusCode;
        
        public RequestException(System.Net.HttpStatusCode httpStatusCode, string message)
            : base(message)
        {
            this.HttpStatusCode = httpStatusCode;
        }
    }
}
