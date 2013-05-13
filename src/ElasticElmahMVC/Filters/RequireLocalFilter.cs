using System.Web.Mvc;

namespace ElasticElmahMVC.Filters
{
    public class RequireLocalFilter : IAuthorizationFilter
    {
        #region IAuthorizationFilter Members

        public void OnAuthorization(AuthorizationContext filterContext)
        {
            if (!filterContext.HttpContext.Request.IsLocal)
            {
                filterContext.Result = new HttpStatusCodeResult(403);
            }
        }

        #endregion
    }
}