using Microsoft.AspNetCore.Mvc;

namespace Senior_Project.Controllers
{
    public class Landing : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
