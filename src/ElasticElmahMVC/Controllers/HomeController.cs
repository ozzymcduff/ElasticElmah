using ElasticElmahMVC.Code;
using Elmah;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ElmahMVC.Controllers
{
    public class HomeController : Controller
    {
        //
        // GET: /Home/

        public ActionResult Index(int? size = null, int? page = null)
        {
            const int _defaultPageSize = 15;
            const int _maximumPageSize = 100;

            var _pageSize = Math.Min(_maximumPageSize, Math.Max(0, size ?? 0));

            if (_pageSize == 0)
            {
                _pageSize = _defaultPageSize;
            }

            var _pageIndex = Math.Max(1, page ?? 0) - 1;
            var errorlog = Helper.GetDefault(HttpContext);
            this.ViewBag.ErrorLog = errorlog;
            var errors = errorlog.GetErrors(_pageIndex, _pageSize);
            return View(new ErrorLogPage(new Elmah.Environment(HttpContext), errors).OnLoad());
        }

        public ActionResult About()
        {
            this.ViewBag.ErrorLog = Helper.GetDefault(HttpContext);
            return View(new AboutModel(new Elmah.Environment(HttpContext)));
        }

        public ActionResult Test()
        {
            throw new TestException();
        }

        //case "test":
        //    ;


    }
}
