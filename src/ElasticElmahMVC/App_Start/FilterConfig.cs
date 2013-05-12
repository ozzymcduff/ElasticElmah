using ElasticElmahMVC.Code.Filters;
using System.Web;
using System.Web.Mvc;

namespace ElmahMVC
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new RequireLocalFilter());
            filters.Add(new HandleErrorAttribute());
        }
    }
}