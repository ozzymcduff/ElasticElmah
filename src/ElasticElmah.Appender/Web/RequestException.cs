using System;

namespace ElasticElmah.Appender.Web
{
    [Serializable]
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
