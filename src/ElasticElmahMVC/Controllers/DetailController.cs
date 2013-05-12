using ElasticElmahMVC.Code;
using Elmah;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ElasticElmahMVC.Controllers
{
    public class DetailController : Controller
    {
        //
        // GET: /Detail/

        public ActionResult Index(string id)
        {
            var errorlog=Helper.GetDefault(HttpContext);
            this.ViewBag.ErrorLog = errorlog;
            return View(new ErrorDetailModel(errorlog.GetError(id),new Elmah.Environment(HttpContext)));
        }

    }
}
