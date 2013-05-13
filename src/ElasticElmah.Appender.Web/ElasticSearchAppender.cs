using System;
using System.Diagnostics;
using System.Security;
using System.Security.Principal;
using System.Threading;
using System.Web;
using log4net.Appender;
using log4net.Core;

namespace ElasticElmah.Appender.Web
{
    public class ElasticSearchAppender : AppenderSkeleton
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
            Repo.Add(loggingEvent);
        }

        //public void HydrateProperties(Exception e, HttpContext context)
        //{
        //    var _exception = e;
        //    Exception baseException = e.GetBaseException();

        //    //
        //    // Load the basic information.
        //    //

        //    //var _hostName = Environment.TryGetMachineName(context);
        //    var _typeName = baseException.GetType().FullName;
        //    var _message = baseException.Message;
        //    var _source = baseException.Source;
        //    var _detail = e.ToString();
        //    var _user = Thread.CurrentPrincipal.Identity.Name;
        //    var _time = DateTime.Now;

        //    //
        //    // If this is an HTTP exception, then get the status code
        //    // and detailed HTML message provided by the host.
        //    //

        //    //HttpException httpException = e as HttpException;

        //    //if (httpException != null)
        //    //{
        //    //    _statusCode = httpException.GetHttpCode();
        //    //    _webHostHtmlMessage = TryGetHtmlErrorMessage(httpException);
        //    //}

        //    //
        //    // If the HTTP context is available, then capture the
        //    // collections that represent the state request as well as
        //    // the user.
        //    //

        //    if (context != null)
        //    {
        //        //IPrincipal webUser = context.User;
        //        //if (webUser != null
        //        //    && Mask.NullString(webUser.Identity.Name).Length > 0)
        //        //{
        //        //    _user = webUser.Identity.Name;
        //        //}

        //        //HttpRequest request = context.Request;

        //        //var _serverVariables = CopyCollection(request.ServerVariables);

        //        //if (_serverVariables != null)
        //        //{
        //        //    // Hack for issue #140:
        //        //    // http://code.google.com/p/elmah/issues/detail?id=140
 
        //        //    const string authPasswordKey = "AUTH_PASSWORD";
        //        //    string authPassword = _serverVariables[authPasswordKey];
        //        //    if (authPassword != null) // yes, mask empty too!
        //        //        _serverVariables[authPasswordKey] = "*****";
        //        //}

        //        //_queryString = CopyCollection(request.QueryString);
        //        //_form = CopyCollection(request.Form);
        //        //_cookies = CopyCollection(request.Cookies);
        //    }
        //}

        //private static string TryGetHtmlErrorMessage(HttpException e)
        //{

        //    try
        //    {
        //        return e.GetHtmlErrorMessage();
        //    }
        //    catch (SecurityException se) 
        //    {
        //        // In partial trust environments, HttpException.GetHtmlErrorMessage() 
        //        // has been known to throw:
        //        // System.Security.SecurityException: Request for the 
        //        // permission of type 'System.Web.AspNetHostingPermission' failed.
        //        // 
        //        // See issue #179 for more background:
        //        // http://code.google.com/p/elmah/issues/detail?id=179
                
        //        //Trace.WriteLine(se);
        //        return null;
        //    }
        //}

    }
}
