using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SchoolScheduleWeb.Models;

namespace SchoolScheduleWeb.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        return RedirectToAction("Index", "Schedule");
    }

    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
