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

        public async Task<ActionResult> Index(string id)
        {
            ErrorLog errorlog = Helper.GetDefault(HttpContext);
            ViewBag.ErrorLog = errorlog;
            var error = await errorlog.GetErrorAsync(id);
            return View(new ErrorDetailModel(error, new Environment(HttpContext)));
        }
    }
}