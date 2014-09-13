using System.Web.Mvc;
using ElasticElmah.Core.ErrorLog;
using ElasticElmahMVC.Code;
using ElasticElmahMVC.Models;
using System.Threading.Tasks;

namespace ElasticElmahMVC.Controllers
{
    public class DetailController : Controller
    {
        //
        // GET: /Detail/

        public ActionResult Index(string id)
        {
            IErrorLog errorlog = Helper.GetDefault(HttpContext);
            ViewBag.ErrorLog = errorlog;
            var error = errorlog.GetError(id);
            return View(new ErrorDetailModel(error, new Environment(HttpContext)));
        }
    }
}