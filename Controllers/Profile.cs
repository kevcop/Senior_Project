using Microsoft.AspNetCore.Mvc;

namespace Senior_Project.Controllers
{
    public class Profile : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
