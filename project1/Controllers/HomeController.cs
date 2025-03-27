using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using project1.Models;

namespace project1.Controllers;


public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        if (User.Identity.IsAuthenticated)
        {
            if (User.IsInRole("HR"))
            {
                return RedirectToAction("Dashboard", "HR");
            }
            else if (User.IsInRole("Employee"))
            {
                return RedirectToAction("Dashboard", "Employee");
            }
        }
        return View();
    }

    [Authorize(Roles = "HR")]
    public IActionResult Privacy()
    {
        if (!User.Identity.IsAuthenticated || !User.IsInRole("HR"))
        {
            return Forbid(); 
        }
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        var requestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        var model = new ErrorViewModel { RequestId = requestId };
        return View(model);
    }
}

