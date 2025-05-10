using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MS.Data;
using MS.Models;

namespace MS.Controllers
{
    public class StudentController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StudentController(ApplicationDbContext context)
        {
            _context = context;
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

        // GET: Student
        public async Task<IActionResult> Index()
        {
            if (!IsAdminOrSuperAdmin()) return AccessDenied();
            return View(await _context.Students.ToListAsync());
        }

        // GET: Student/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (!IsAdminOrSuperAdmin()) return AccessDenied();
            if (id == null)
            {
                return NotFound();
            }

            var student = await _context.Students
                .FirstOrDefaultAsync(m => m.Id == id);
            if (student == null)
            {
                return NotFound();
            }

            return View(student);
        }

        // GET: Student/Create
        public IActionResult Create()
        {
            if (!IsAdminOrSuperAdmin()) return AccessDenied();
            return View();
        }

        // POST: Student/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Session,Degree,Section,Phone,Email,Address,Age,Gender,CNIC,RollNumber")] Student student)
        {
            if (!IsAdminOrSuperAdmin()) return AccessDenied();
            try
            {
                if (ModelState.IsValid)
                {
                    // Generate roll number
                    var lastStudent = await _context.Students
                        .Where(s => s.Session == student.Session && s.Degree == student.Degree)
                        .OrderByDescending(s => s.RollNumber)
                        .FirstOrDefaultAsync();

                    int sequence = 1;
                    if (lastStudent != null)
                    {
                        var parts = lastStudent.RollNumber.Split('-');
                        if (parts.Length == 3 && int.TryParse(parts[2], out int lastSequence))
                        {
                            sequence = lastSequence + 1;
                        }
                    }

                    student.RollNumber = $"{student.Session}-{student.Degree}-{sequence:D2}";

                    _context.Add(student);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }

                // If we got this far, something failed, log the ModelState errors
                foreach (var modelState in ModelState.Values)
                {
                    foreach (var error in modelState.Errors)
                    {
                        System.Diagnostics.Debug.WriteLine($"Model Error: {error.ErrorMessage}");
                    }
                }
                return View(student);
            }
            catch (Exception ex)
            {
                // Log the exception
                System.Diagnostics.Debug.WriteLine($"Exception in Create: {ex.Message}");
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
                return View(student);
            }
        }

        // GET: Student/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (!IsAdminOrSuperAdmin()) return AccessDenied();
            if (id == null)
            {
                return NotFound();
            }

            var student = await _context.Students.FindAsync(id);
            if (student == null)
            {
                return NotFound();
            }
            return View(student);
        }

        // POST: Student/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,RollNumber,Phone,Email,Address,Age,Gender,CNIC,Session,Degree")] Student student)
        {
            if (!IsAdminOrSuperAdmin()) return AccessDenied();
            if (id != student.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(student);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!StudentExists(student.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(student);
        }

        // GET: Student/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (!IsAdminOrSuperAdmin()) return AccessDenied();
            if (id == null)
            {
                return NotFound();
            }

            var student = await _context.Students
                .FirstOrDefaultAsync(m => m.Id == id);
            if (student == null)
            {
                return NotFound();
            }

            return View(student);
        }

        // POST: Student/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!IsAdminOrSuperAdmin()) return AccessDenied();
            var student = await _context.Students.FindAsync(id);
            if (student != null)
            {
                _context.Students.Remove(student);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // API Endpoints
        [HttpGet]
        [Route("api/[controller]")]
        public async Task<ActionResult<IEnumerable<Student>>> GetStudents()
        {
            if (!IsAdminOrSuperAdmin()) return Forbid();
            return await _context.Students.ToListAsync();
        }

        [HttpGet]
        [Route("api/[controller]/filter")]
        public async Task<ActionResult<IEnumerable<Student>>> FilterStudents([FromQuery] string? session, [FromQuery] string? degree)
        {
            if (!IsAdminOrSuperAdmin()) return Forbid();
            var query = _context.Students.AsQueryable();

            if (!string.IsNullOrEmpty(session))
            {
                query = query.Where(s => s.Session == session);
            }

            if (!string.IsNullOrEmpty(degree))
            {
                query = query.Where(s => s.Degree == degree);
            }

            return await query.ToListAsync();
        }

        private bool StudentExists(int id)
        {
            return _context.Students.Any(e => e.Id == id);
        }
    }
} 