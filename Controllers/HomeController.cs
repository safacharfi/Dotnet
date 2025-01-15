using Microsoft.AspNetCore.Mvc;

namespace sav.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            // Charge la page d'accueil (Welcome Page)
            return View();
        }

        public IActionResult ClientDashboard()
        {
            // Redirige vers la page Client Dashboard
            return View("~/Views/Client/ClientHomePage.cshtml");
        }

        public IActionResult AdminDashboard()
        {
            // Redirige vers la page Admin Dashboard
            return View("~/Views/Client/AdminDashboard.cshtml");
        }
    }
}
