using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ElasticElmahMVC.Filters
{
    public class RequireLocalFilter : IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationContext filterContext)
        {
            if (!filterContext.HttpContext.Request.IsLocal) 
            {
                filterContext.Result = new HttpStatusCodeResult(403);
            }
        }
    }
}