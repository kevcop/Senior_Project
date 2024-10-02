using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ActionConstraints;

namespace Senior_Project.Controllers
{
    public class Login:Controller
    {
        public IActionResult Index()
        {
            return View();
        }

    }
}
