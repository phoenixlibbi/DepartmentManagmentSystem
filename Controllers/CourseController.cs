using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MS.Data;
using MS.Models;
using Microsoft.Extensions.Logging;

namespace MS.Controllers
{
    public class CourseController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CourseController> _logger;

        public CourseController(ApplicationDbContext context, ILogger<CourseController> logger)
        {
            _context = context;
            _logger = logger;
        }

        private bool IsAdminOrSuperAdmin()
        {
            var role = HttpContext.Session.GetString("Role");
            return role == "SUPER_ADMIN" || role == "ADMIN";
        }
        private IActionResult AccessDenied()
        {
            return View("~/Views/Shared/AccessDenied.cshtml");
        }

        // GET: Course
        public async Task<IActionResult> Index()
        {
            if (!IsAdminOrSuperAdmin()) return AccessDenied();
            try
            {
                return View(await _context.Courses.ToListAsync());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving courses");
                TempData["ErrorMessage"] = "An error occurred while retrieving courses. Please try again later.";
                return View(new List<Course>());
            }
        }

        // GET: Course/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var course = await _context.Courses
                    .FirstOrDefaultAsync(m => m.Id == id);
                if (course == null)
                {
                    return NotFound();
                }

                return View(course);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving course details for ID {CourseId}", id);
                TempData["ErrorMessage"] = "An error occurred while retrieving course details. Please try again later.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Course/Create
        public IActionResult Create()
        {
            if (!IsAdminOrSuperAdmin()) return AccessDenied();
            return View();
        }

        // POST: Course/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Code,CreditHours,Degree")] Course course)
        {
            if (!IsAdminOrSuperAdmin()) return AccessDenied();
            _logger.LogInformation("Create action started. Course data: {@Course}", course);
            
            try
            {
                if (ModelState.IsValid)
                {
                    _context.Add(course);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Course created successfully: {@Course}", course);
                    TempData["SuccessMessage"] = "Course created successfully.";
                    return RedirectToAction(nameof(Index));
                }
                return View(course);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating course");
                TempData["ErrorMessage"] = "An error occurred while creating the course. Please try again.";
                return View(course);
            }
        }

        // GET: Course/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (!IsAdminOrSuperAdmin()) return AccessDenied();
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var course = await _context.Courses.FindAsync(id);
                if (course == null)
                {
                    return NotFound();
                }
                return View(course);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving course for edit with ID {CourseId}", id);
                TempData["ErrorMessage"] = "An error occurred while retrieving the course. Please try again later.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Course/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Code,CreditHours,Degree")] Course course)
        {
            if (!IsAdminOrSuperAdmin()) return AccessDenied();
            if (id != course.Id)
            {
                return NotFound();
            }

            try
            {
                if (ModelState.IsValid)
                {
                    try
                    {
                        _context.Update(course);
                        await _context.SaveChangesAsync();
                        TempData["SuccessMessage"] = "Course updated successfully.";
                        return RedirectToAction(nameof(Index));
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!CourseExists(course.Id))
                        {
                            return NotFound();
                        }
                        else
                        {
                            throw;
                        }
                    }
                }
                return View(course);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating course with ID {CourseId}", id);
                TempData["ErrorMessage"] = "An error occurred while updating the course. Please try again.";
                return View(course);
            }
        }

        // GET: Course/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (!IsAdminOrSuperAdmin()) return AccessDenied();
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var course = await _context.Courses
                    .FirstOrDefaultAsync(m => m.Id == id);
                if (course == null)
                {
                    return NotFound();
                }

                return View(course);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving course for deletion with ID {CourseId}", id);
                TempData["ErrorMessage"] = "An error occurred while retrieving the course. Please try again later.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Course/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!IsAdminOrSuperAdmin()) return AccessDenied();
            try
            {
                var course = await _context.Courses.FindAsync(id);
                if (course == null)
                {
                    return NotFound();
                }

                // Check if there are any related exam seatings
                var hasExamSeatings = await _context.ExamSeatings
                    .AnyAsync(es => es.CourseId == id);

                if (hasExamSeatings)
                {
                    TempData["ErrorMessage"] = "Cannot delete this course because it has associated exam seatings. Please delete the exam seatings first.";
                    return RedirectToAction(nameof(Index));
                }

                _context.Courses.Remove(course);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Course deleted successfully.";
                
                // Force reload the page with updated data
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error while deleting course with ID {CourseId}", id);
                TempData["ErrorMessage"] = "An error occurred while deleting the course. This course may have associated records that need to be deleted first.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error deleting course with ID {CourseId}", id);
                TempData["ErrorMessage"] = "An unexpected error occurred. Please try again later.";
                return RedirectToAction(nameof(Index));
            }
        }

        // API Endpoints
        [HttpGet]
        [Route("api/[controller]")]
        public async Task<ActionResult<IEnumerable<Course>>> GetCourses()
        {
            if (!IsAdminOrSuperAdmin()) return Forbid();
            return await _context.Courses.ToListAsync();
        }

        private bool CourseExists(int id)
        {
            return _context.Courses.Any(e => e.Id == id);
        }
    }
} 