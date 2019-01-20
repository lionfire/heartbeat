using Microsoft.AspNetCore.Mvc;

namespace LionFire.Heartbeat.Samples.AspNetCore.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
