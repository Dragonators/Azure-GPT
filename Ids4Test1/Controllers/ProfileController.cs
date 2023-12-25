using Microsoft.AspNetCore.Mvc;

namespace Ids4Test1.Controllers
{
    public class ProfileController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
