using System.Reflection;
using System.Web.Mvc;
using log4net;

namespace ElasticElmahMVC.Filters
{
    public class HandleWithLog4netFilter : IActionFilter
    {
        protected static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        #region IActionFilter Members

        public void OnActionExecuted(ActionExecutedContext filterContext)
        {
            if (log.IsErrorEnabled)
            {
                if (filterContext.Exception != null && !filterContext.ExceptionHandled)
                {
                    log.Error(filterContext.Exception);
                    filterContext.ExceptionHandled = true;
                    filterContext.Result = new HttpStatusCodeResult(500);
                }
            }
        }

        public void OnActionExecuting(ActionExecutingContext filterContext)
        {
        }

        #endregion
    }
}