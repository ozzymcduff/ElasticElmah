using System;
using System.Diagnostics;
using System.Security;
using System.Security.Principal;
using System.Threading;
using System.Web;
using System.Linq;
using log4net.Appender;
using log4net.Core;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace ElasticElmah.Appender
{
    public class ElasticSearchWebAppender : AppenderSkeleton
    {
        private ElasticSearchRepository _repo;
        public string ConnectionString { get; set; }
        private static readonly object _lockObj = new object();
        protected virtual ElasticSearchRepository Repo
        {
            get
            {
                lock (_lockObj)
                {
                    if (_repo != null)
                    {
                        return _repo;
                    }
                    _repo = new ElasticSearchRepository(ConnectionString);
                    _repo.CreateIndexIfNotExists();
                    return _repo;
                }
            }
        }
        /// <summary>
        /// Add a log event to the ElasticSearch Repo
        /// </summary>
        /// <param name="loggingEvent"></param>
        protected override void Append(LoggingEvent loggingEvent)
        {
            HydrateProperties(loggingEvent, HttpContext.Current != null
                ? new HttpContextWrapper(HttpContext.Current)
                : null);
            Repo.Add(loggingEvent);
        }

        public class ErrorCodeAndHtmlMessage
        {
            public int StatusCode { get; set; }
            public string WebHostHtmlMessage { get; set; }
            public ErrorCodeAndHtmlMessage(int statusCode, string webHostHtmlMessage)
            {
                StatusCode = statusCode;
                WebHostHtmlMessage = webHostHtmlMessage;
            }
            public ErrorCodeAndHtmlMessage()
            {
            }
        }

        public void HydrateProperties(LoggingEvent e, HttpContextBase context)
        {
            var errors = new List<ErrorCodeAndHtmlMessage>();
            if (context.AllErrors != null)
            {
                for (int i = 0; i < context.AllErrors.Length; i++)
                {
                    var error = context.AllErrors[i];
                    if (error is HttpException)
                    {
                        var httpEx = error as HttpException;
                        var statusCode = httpEx.GetHttpCode();
                        var webHostHtmlMessage = TryGetHtmlErrorMessage(httpEx);
                        errors.Add(new ErrorCodeAndHtmlMessage(statusCode, webHostHtmlMessage));
                    }
                }
                if (errors.Any())
                    e.Properties["httpErrors"] = errors;
            }
            if (context != null)
            {
                var request = context.Request;

                var _serverVariables = MapToJson(CopyCollection(request.ServerVariables));

                if (_serverVariables != null)
                {
                    // Hack for issue #140:
                    // http://code.google.com/p/elmah/issues/detail?id=140

                    const string authPasswordKey = "AUTHPASSWORD";
                    var authPassword = _serverVariables[authPasswordKey];
                    if (authPassword != null) // yes, mask empty too!
                        _serverVariables[authPasswordKey] = "*****";
                    e.Properties["serverVariables"] = _serverVariables;
                }
                e.Properties["queryString"] = CopyCollection(request.QueryString);
                e.Properties["form"] = CopyCollection(request.Form);
                e.Properties["cookies"] = CopyCollection(request.Cookies);
            }
        }

        private Dictionary<string, object> MapToJson(Dictionary<string, object> dictionary)
        {
            return dictionary.ToDictionary(kv => JsonNameConvention(kv.Key), kv => kv.Value,
                StringComparer.InvariantCultureIgnoreCase);
        }

        private string JsonNameConvention(string key)
        {
            return string.Join("", (key ?? string.Empty).ToLower().Split('_')
                .Select(k => FirstLetterToUpper(k)).ToArray());
        }

        private string FirstLetterToUpper(string val)
        {
            if (val.Length > 0)
                return val.First().ToString().ToUpper() + (val.Length > 1 ? val.Substring(1) : string.Empty);
            return string.Empty;
        }

        private Dictionary<string, object> CopyCollection(HttpCookieCollection httpCookieCollection)
        {
            var dic = new Dictionary<string, object>();
            foreach (var key in httpCookieCollection.AllKeys)
            {
                dic[key] = httpCookieCollection[key].Value;
            }
            return dic;
        }

        private Dictionary<string, object> CopyCollection(System.Collections.Specialized.NameValueCollection nameValueCollection)
        {
            var dic = new Dictionary<string, object>();
            foreach (var key in nameValueCollection.AllKeys)
            {
                dic[key] = nameValueCollection[key];
            }
            return dic;
        }

        private static string TryGetHtmlErrorMessage(HttpException e)
        {

            try
            {
                return e.GetHtmlErrorMessage();
            }
            catch (SecurityException se)
            {
                // In partial trust environments, HttpException.GetHtmlErrorMessage() 
                // has been known to throw:
                // System.Security.SecurityException: Request for the 
                // permission of type 'System.Web.AspNetHostingPermission' failed.
                // 
                // See issue #179 for more background:
                // http://code.google.com/p/elmah/issues/detail?id=179

                //Trace.WriteLine(se);
                return null;
            }
        }

    }
}
