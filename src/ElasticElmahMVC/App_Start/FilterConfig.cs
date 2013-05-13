using System.Web.Mvc;
using ElasticElmahMVC.Filters;

namespace ElmahMVC
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new RequireLocalFilter());
            filters.Add(new HandleWithLog4netFilter());
            filters.Add(new HandleErrorAttribute());
        }
    }
}