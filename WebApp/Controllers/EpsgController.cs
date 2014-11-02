using Microsoft.AspNet.Mvc;
using Pigeoid.Epsg;
using WebApp.Models;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace WebApp.Controllers
{
    public class EpsgController : Controller
    {

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Crs(int id)
        {
            return View(EpsgMicroDatabase.Default.GetCrs(id));
        }

    }
}
