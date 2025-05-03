using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MS.Data;
using MS.Models;

namespace MS.Controllers
{
    public class DataController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DataController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            try
            {
                // Test database connection
                _context.Database.OpenConnection();
                _context.Database.CloseConnection();
                TempData["SuccessMessage"] = "Database connected successfully!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Database connection failed: {ex.Message}";
            }

            var viewModel = new DatabaseDataViewModel
            {
                Courses = _context.Courses.ToList(),
                Rooms = _context.Rooms.ToList(),
                Students = _context.Students.ToList(),
                ExamSeatings = _context.ExamSeatings
                    .Include(es => es.Course)
                    .Include(es => es.Room)
                    .Include(es => es.Student)
                    .ToList()
            };

            return View(viewModel);
        }

        public IActionResult Years()
        {
            var currentYear = DateTime.Now.Year;
            var years = Enumerable.Range(2000, currentYear - 2000 + 1).ToList();
            return View(years);
        }
    }

    public class DatabaseDataViewModel
    {
        public List<Course> Courses { get; set; }
        public List<Room> Rooms { get; set; }
        public List<Student> Students { get; set; }
        public List<ExamSeating> ExamSeatings { get; set; }
    }
} 