using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MS.Data;
using MS.Models;
using Microsoft.Extensions.Logging;

namespace MS.Controllers
{
    public class RoomController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<RoomController> _logger;

        public RoomController(ApplicationDbContext context, ILogger<RoomController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Room
        public async Task<IActionResult> Index()
        {
            return View(await _context.Rooms.ToListAsync());
        }

        // GET: Room/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var room = await _context.Rooms
                .FirstOrDefaultAsync(m => m.Id == id);
            if (room == null)
            {
                return NotFound();
            }

            return View(room);
        }

        // GET: Room/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Room/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("RoomNumber,Capacity,IsBooked")] Room room)
        {
            _logger.LogInformation("Create action started. Room data: {@Room}", room);

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Add(room);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Room created successfully: {@Room}", room);

                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = true });
                    }
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating room: {@Room}", room);
                    ModelState.AddModelError("", "An error occurred while saving the room. Please try again.");
                }
            }
            else
            {
                _logger.LogWarning("Model state is invalid. Errors: {@Errors}",
                    ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
            }

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = false, errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
            }
            return View(room);
        }

        // GET: Room/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var room = await _context.Rooms.FindAsync(id);
            if (room == null)
            {
                return NotFound();
            }
            return View(room);
        }

        // POST: Room/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,RoomNumber,Capacity,IsBooked")] Room room)
        {
            if (id != room.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(room);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RoomExists(room.Id))
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
            return View(room);
        }

        // GET: Room/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                _logger.LogWarning("Delete action called with null id");
                return NotFound();
            }

            var room = await _context.Rooms
                .FirstOrDefaultAsync(m => m.Id == id);
            if (room == null)
            {
                _logger.LogWarning("Room not found for id: {Id}", id);
                return NotFound();
            }

            return View(room);
        }

        // POST: Room/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var room = await _context.Rooms.FindAsync(id);
                if (room == null)
                {
                    _logger.LogWarning("Room not found for deletion, id: {Id}", id);
                    return NotFound();
                }

                _context.Rooms.Remove(room);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Room deleted successfully, id: {Id}", id);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting room with id: {Id}", id);
                ModelState.AddModelError("", "An error occurred while deleting the room. Please try again.");
                return View(await _context.Rooms.FindAsync(id));
            }
        }

        private bool RoomExists(int id)
        {
            return _context.Rooms.Any(e => e.Id == id);
        }
    }
} 