using System.Web.Mvc;
using ElasticElmahMVC.Code;
using Elmah;

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