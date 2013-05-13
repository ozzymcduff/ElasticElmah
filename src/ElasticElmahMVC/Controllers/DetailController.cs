using System.Web.Mvc;
using ElasticElmah.Core.ErrorLog;
using ElasticElmahMVC.Code;
using ElasticElmahMVC.Models;

namespace ElasticElmahMVC.Controllers
{
    public class DetailController : Controller
    {
        //
        // GET: /Detail/

        public ActionResult Index(string id)
        {
            ErrorLog errorlog = Helper.GetDefault(HttpContext);
            ViewBag.ErrorLog = errorlog;
            return View(new ErrorDetailModel(errorlog.GetError(id), new Environment(HttpContext)));
        }
    }
}