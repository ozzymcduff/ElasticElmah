using System.Web.Mvc;
using ElasticElmah.Core.ErrorLog;
using ElasticElmahMVC.Code;
using System.Threading.Tasks;
using System.Linq;
namespace ElasticElmahMVC.Controllers
{
    public class RssController : Controller
    {
        //
        // GET: /Rss/

        public ActionResult Index()
        {
            var errorlog = Helper.GetDefault(HttpContext);
            var errors = errorlog.GetErrors(0, ErrorDigestRssHandler.pageSize);
            return new ErrorRssHandler(new Environment(HttpContext), errors.Hits.ToArray());
        }

        public ActionResult Digest()
        {
            var errorlog = Helper.GetDefault(HttpContext);
            var errors = errorlog.GetErrors(0, ErrorDigestRssHandler.pageSize);
            return new ErrorDigestRssHandler(new Environment(HttpContext), errors.Hits.ToArray());
        }
    }
}