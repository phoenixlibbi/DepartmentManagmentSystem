using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MS.Data;
using MS.Models;

namespace MS.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _context;

    public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    [Route("api/Home/statistics")]
    public async Task<IActionResult> GetStatistics()
    {
        var statistics = new
        {
            studentCount = await _context.Students.CountAsync(),
            courseCount = await _context.Courses.CountAsync(),
            roomCount = await _context.Rooms.CountAsync(),
            examCount = await _context.ExamSeatings
                .Where(e => e.ExamDate.Date >= DateTime.Today)
                .Select(e => e.ExamDate.Date)
                .Distinct()
                .CountAsync()
        };

        return Json(statistics);
    }
}
