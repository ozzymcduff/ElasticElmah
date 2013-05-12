using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;

namespace ElasticElmahMVC.Filters
{
    public class HandleWithLog4netFilter:IActionFilter
    {
        protected static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

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
    }
}