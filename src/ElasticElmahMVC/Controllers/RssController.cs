using System.Web.Mvc;
using ElasticElmah.Core.ErrorLog;
using ElasticElmahMVC.Code;

namespace ElasticElmahMVC.Controllers
{
    public class RssController : Controller
    {
        //
        // GET: /Rss/

        public ActionResult Index()
        {
            ErrorLog errorlog = Helper.GetDefault(HttpContext);
            ErrorLog.Errors errors = errorlog.GetErrors(0, ErrorDigestRssHandler.pageSize);
            return new ErrorRssHandler(new Environment(HttpContext), errors.Entries);
        }

        public ActionResult Digest()
        {
            ErrorLog errorlog = Helper.GetDefault(HttpContext);
            ErrorLog.Errors errors = errorlog.GetErrors(0, ErrorDigestRssHandler.pageSize);
            return new ErrorDigestRssHandler(new Environment(HttpContext), errors.Entries);
        }
    }
}