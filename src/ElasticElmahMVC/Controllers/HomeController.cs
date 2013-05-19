using System;
using System.Web.Mvc;
using ElasticElmah.Core;
using ElasticElmah.Core.ErrorLog;
using ElasticElmahMVC.Code;
using ElasticElmahMVC.Models;
using Environment = ElasticElmahMVC.Code.Environment;
using System.Threading.Tasks;

namespace ElasticElmahMVC.Controllers
{
    public class HomeController : Controller
    {
        //
        // GET: /Home/

        public async Task<ActionResult> Index(int? size = null, int? page = null)
        {
            const int _defaultPageSize = 15;
            const int _maximumPageSize = 100;

            int _pageSize = Math.Min(_maximumPageSize, Math.Max(0, size ?? 0));

            if (_pageSize == 0)
            {
                _pageSize = _defaultPageSize;
            }

            int _pageIndex = Math.Max(1, page ?? 0) - 1;
            var errorlog = Helper.GetDefault(HttpContext);
            ViewBag.ErrorLog = errorlog;
            var errors = await errorlog.GetErrorsAsync(_pageIndex, _pageSize);
            return View(new ErrorLogPage(new Environment(HttpContext), errors).OnLoad());
        }

        public ActionResult About()
        {
            ViewBag.ErrorLog = Helper.GetDefault(HttpContext);
            return View(new AboutModel(new Environment(HttpContext)));
        }

        public ActionResult Test()
        {
            throw new TestException();
        }

        //case "test":
        //    ;
    }
}