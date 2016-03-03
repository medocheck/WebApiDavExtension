using System.Web.Mvc;

namespace WebApiDavExtension.example.Controllers
{
    public class HomeController : Controller
    {
        // GET: Home
        public ActionResult Index()
        {
	        //var calDavConfig = (CalDavConfiguration)ConfigurationManager.GetSection("calDavConfiguration");
            return View();
        }
    }
}