using Mark2MVC.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Mark2MVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult CheckAuthentication()
        {
            // Check if the JWT token is present in cookies
            if (Request.Cookies.ContainsKey("jwt"))
            {
                // Token is present, redirect to Exam Controller's Index action
                return RedirectToAction("Index", "Exam");
            }
            else
            {
                // Token is not present, directly return the alert message
                ViewData["Message"] = "Please Log in to access this feature.";
                return View("Index");
            }
        }

        [HttpPost]
        public IActionResult CheckAuthentication1()
        {
            // Check if the JWT token is present in cookies
            if (Request.Cookies.ContainsKey("jwt"))
            {
                // Token is present, redirect to Exam Controller's Index action
                return RedirectToAction("GetByEmail", "Test");
            }
            else
            {
                // Token is not present, directly return the alert message
                ViewData["Message"] = "Please Log in to access this feature.";
                return View("Index");
            }
        }

        public IActionResult AboutUs()
        {
            return View();
        }

        //used to render as a view page and defines that nothing should be store in cache
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}