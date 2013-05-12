using ElasticElmahMVC.Code;
using Elmah;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ElmahMVC.Controllers
{
    public class RssController : Controller
    {
        //
        // GET: /Rss/

        public ActionResult Index()
        {
            var errorlog = Helper.GetDefault(HttpContext);
            var errors = errorlog.GetErrors(0, ErrorDigestRssHandler.pageSize);
            return new ErrorRssHandler(new Elmah.Environment(HttpContext), errors.Entries);
        }
        public ActionResult Digest()
        {
            var errorlog = Helper.GetDefault(HttpContext);
            var errors = errorlog.GetErrors(0, ErrorDigestRssHandler.pageSize);
            return new ErrorDigestRssHandler(new Elmah.Environment(HttpContext), errors.Entries);
        }

    }
}
