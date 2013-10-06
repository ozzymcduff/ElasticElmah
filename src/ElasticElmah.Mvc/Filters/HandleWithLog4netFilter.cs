using System.Reflection;
using System.Web.Mvc;
using log4net;
using log4net.Core;
using log4net.Repository.Hierarchy;
using ElasticElmah.Appender;

namespace ElasticElmahMVC.Filters
{
    public class HandleWithLog4netFilter : IActionFilter
    {
        protected static readonly ILogger log = LoggerManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType.Assembly,
            MethodBase.GetCurrentMethod().DeclaringType.FullName);

        public void OnActionExecuted(ActionExecutedContext filterContext)
        {
            if (filterContext.Exception != null && !filterContext.ExceptionHandled && log.IsEnabledFor(Level.Error))
            {
                var loggingevent = new LoggingEvent(typeof(Logger),
                    log.Repository, log.Name, Level.Error, "Unhandled exception", filterContext.Exception);
                ElasticSearchWebAppender.AddHttpContextProperties(loggingevent, filterContext.HttpContext);
                log.Log(loggingevent);
                filterContext.ExceptionHandled = true;
                filterContext.Result = new HttpStatusCodeResult(500);
            }
        }

        public void OnActionExecuting(ActionExecutingContext filterContext)
        {
        }
    }
}