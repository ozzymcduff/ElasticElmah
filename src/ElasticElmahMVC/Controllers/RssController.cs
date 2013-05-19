using System.Web.Mvc;
using ElasticElmah.Core.ErrorLog;
using ElasticElmahMVC.Code;
using System.Threading.Tasks;

namespace ElasticElmahMVC.Controllers
{
    public class RssController : Controller
    {
        //
        // GET: /Rss/

        public async Task<ActionResult> Index()
        {
            var errorlog = Helper.GetDefault(HttpContext);
            var errors = await errorlog.GetErrorsAsync(0, ErrorDigestRssHandler.pageSize);
            return new ErrorRssHandler(new Environment(HttpContext), errors.Entries);
        }

        public async Task<ActionResult> Digest()
        {
            var errorlog = Helper.GetDefault(HttpContext);
            var errors = await errorlog.GetErrorsAsync(0, ErrorDigestRssHandler.pageSize);
            return new ErrorDigestRssHandler(new Environment(HttpContext), errors.Entries);
        }
    }
}