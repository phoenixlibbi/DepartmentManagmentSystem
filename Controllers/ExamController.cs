using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MS.Data;
using MS.Models;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.Text;
using Microsoft.Extensions.Logging;

namespace MS.Controllers
{
    public class ExamController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ExamController> _logger;

        public ExamController(ApplicationDbContext context, ILogger<ExamController> logger)
        {
            _context = context;
            _logger = logger;
        }

        private bool IsExamAccessAllowed()
        {
            var role = HttpContext.Session.GetString("Role");
            return role == "SUPER_ADMIN" || role == "ADMIN" || role == "CLERK";
        }

        private IActionResult AccessDenied()
        {
            return View("~/Views/Shared/AccessDenied.cshtml");
        }

        // GET: Exam
        public IActionResult Index()
        {
            if (!IsExamAccessAllowed()) return AccessDenied();
            return View();
        }

        // API Endpoints
        [Route("api/Exam/sessions")]
        public async Task<IActionResult> GetSessions()
        {
            if (!IsExamAccessAllowed()) return AccessDenied();
            
            // Get distinct session years from students table
            var sessions = await _context.Students
                .Select(s => s.Session)
                .Distinct()
                .OrderByDescending(s => s)
                .Select(s => new { value = s, text = s })
                .ToListAsync();

            return Json(sessions);
        }

        [Route("api/Exam/courses")]
        public async Task<IActionResult> GetCourses([FromQuery] string degree)
        {
            if (!IsExamAccessAllowed()) return AccessDenied();
            var courses = await _context.Courses
                .Select(c => new { id = c.Id, code = c.Code, name = c.Name })
                .ToListAsync();

            return Json(courses);
        }

        [Route("api/Exam/rooms")]
        public async Task<IActionResult> GetRooms()
        {
            if (!IsExamAccessAllowed()) return AccessDenied();
            var rooms = await _context.Rooms
                .Where(r => !r.IsBooked)
                .Select(r => new { id = r.Id, roomNumber = r.RoomNumber, capacity = r.Capacity })
                .ToListAsync();

            return Json(rooms);
        }

        [Route("api/Exam/list")]
        public async Task<IActionResult> GetExamList()
        {
            if (!IsExamAccessAllowed()) return AccessDenied();
            var exams = await _context.ExamSeatings
                .Include(e => e.Course)
                .Include(e => e.Room)
                .GroupBy(e => new { e.CourseId, e.RoomId, e.ExamDate, e.ExamTime, e.PaperTotalTime })
                .Select(g => new
                {
                    id = g.First().Id,
                    courseCode = g.First().Course.Code,
                    courseName = g.First().Course.Name,
                    roomNumber = g.First().Room.RoomNumber,
                    examDate = g.First().ExamDate,
                    examTime = g.First().ExamTime.ToString(@"hh\:mm"),
                    paperTotalTime = g.First().PaperTotalTime,
                    studentCount = g.Count()
                })
                .OrderByDescending(e => e.examDate)
                .ToListAsync();

            return Json(exams);
        }

        [Route("api/Exam/sections")]
        public async Task<IActionResult> GetSections([FromQuery] string session, [FromQuery] string degree)
        {
            if (!IsExamAccessAllowed()) return AccessDenied();
            
            // Get distinct sections from students table based on session and degree
            var sections = await _context.Students
                .Where(s => s.Session == session && s.Degree == degree)
                .Select(s => s.Section)
                .Distinct()
                .OrderBy(s => s)
                .Select(s => new { value = s, text = $"Section {s}" })
                .ToListAsync();

            return Json(sections);
        }

        [HttpPost]
        [Route("api/Exam/arrange-seating")]
        public async Task<IActionResult> ArrangeSeating([FromBody] ExamArrangementRequest request)
        {
            if (!IsExamAccessAllowed()) return AccessDenied();
            try
            {
                // Validate request
                if (request == null)
                {
                    return BadRequest(new { success = false, message = "Invalid request data" });
                }

                if (string.IsNullOrEmpty(request.Session) || 
                    string.IsNullOrEmpty(request.Degree) || 
                    string.IsNullOrEmpty(request.Section) || 
                    request.CourseId <= 0 || 
                    request.RoomId <= 0)
                {
                    return BadRequest(new { success = false, message = "All fields are required" });
                }

                // Validate exam date and time
                var examDateTime = request.ExamDate.Add(request.ExamTime);
                if (examDateTime <= DateTime.Now)
                {
                    return BadRequest(new { success = false, message = "Exam date and time must be in the future" });
                }

                // Validate room capacity and availability
                var room = await _context.Rooms.FindAsync(request.RoomId);
                if (room == null)
                {
                    return BadRequest(new { success = false, message = "Room not found" });
                }

                // Check if room is booked for the same date and time
                var isRoomBooked = await _context.ExamSeatings
                    .AnyAsync(e => e.RoomId == request.RoomId && 
                                 e.ExamDate.Date == request.ExamDate.Date &&
                                 e.ExamTime == request.ExamTime);

                if (isRoomBooked)
                {
                    return BadRequest(new { success = false, message = "Room is already booked for another exam at this time" });
                }

                var students = await _context.Students
                    .Where(s => s.Session == request.Session && 
                               s.Degree == request.Degree && 
                               s.Section == request.Section)
                    .ToListAsync();

                if (!students.Any())
                {
                    return BadRequest(new { success = false, message = "No students found for the selected session, degree, and section" });
                }

                if (students.Count > room.Capacity)
                {
                    return BadRequest(new { success = false, message = "Room capacity is less than number of students" });
                }

                // Create seating arrangement
                var examSeatings = new List<ExamSeating>();
                var rows = (int)Math.Ceiling(students.Count / 4.0); // 4 seats per row
                var random = new Random();
                var shuffledStudents = students.OrderBy(x => random.Next()).ToList();

                for (int i = 0; i < shuffledStudents.Count; i++)
                {
                    var row = i / 4 + 1;
                    var seat = i % 4 + 1;
                    var seating = new ExamSeating
                    {
                        RoomId = room.Id,
                        CourseId = request.CourseId,
                        StudentId = shuffledStudents[i].Id,
                        SeatNumber = $"R{row}S{seat}", // Row and Seat number
                        ExamDate = request.ExamDate,
                        ExamTime = request.ExamTime,
                        PaperTotalTime = request.PaperTotalTime,
                        IsPresent = false
                    };
                    examSeatings.Add(seating);
                }

                _context.ExamSeatings.AddRange(examSeatings);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Exam seating arranged successfully!" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error arranging exam seating");
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [Route("api/Exam/seating-plan-pdf")]
        public async Task<IActionResult> GetSeatingPlanPdf([FromQuery] int examId)
        {
            if (!IsExamAccessAllowed()) return AccessDenied();
            var examSeating = await _context.ExamSeatings
                .Include(e => e.Student)
                .Include(e => e.Course)
                .Include(e => e.Room)
                .FirstOrDefaultAsync(e => e.Id == examId);

            if (examSeating == null)
            {
                return NotFound("No seating arrangement found");
            }

            // Get all seatings for this exam (same course, room, and date)
            var examSeatings = await _context.ExamSeatings
                .Include(e => e.Student)
                .Where(e => e.CourseId == examSeating.CourseId && 
                           e.RoomId == examSeating.RoomId && 
                           e.ExamDate == examSeating.ExamDate)
                .ToListAsync();

            // Get the room capacity
            int roomCapacity = examSeating.Room.Capacity;
            int columns = 4;
            int rows = (int)Math.Ceiling(roomCapacity / (double)columns);

            // Randomly assign students to seats (use seat number order if already assigned)
            var random = new Random();
            var shuffledSeatings = examSeatings.OrderBy(e => e.SeatNumber).ToList();
            if (shuffledSeatings.Any(s => string.IsNullOrEmpty(s.SeatNumber)))
            {
                // If seat numbers are not assigned, assign randomly
                var shuffledStudents = examSeatings.Select(e => e.Student).OrderBy(x => random.Next()).ToList();
                shuffledSeatings.Clear();
                for (int i = 0; i < shuffledStudents.Count; i++)
                {
                    var row = i / columns + 1;
                    var seat = i % columns + 1;
                    var seating = new ExamSeating
                    {
                        RoomId = examSeating.RoomId,
                        CourseId = examSeating.CourseId,
                        StudentId = shuffledStudents[i].Id,
                        Student = shuffledStudents[i],
                        SeatNumber = $"R{row}S{seat}",
                        ExamDate = examSeating.ExamDate
                    };
                    shuffledSeatings.Add(seating);
                }
            }

            using (MemoryStream ms = new MemoryStream())
            {
                Document document = new Document(PageSize.A4, 25, 25, 30, 30);
                PdfWriter writer = PdfWriter.GetInstance(document, ms);
                document.Open();

                // White Board at the top
                var whiteBoardFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14);
                var whiteBoard = new Paragraph("White Board", whiteBoardFont)
                {
                    Alignment = Element.ALIGN_CENTER
                };
                var whiteBoardCell = new PdfPCell(new Phrase("White Board", whiteBoardFont))
                {
                    Colspan = columns,
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    Padding = 8,
                    Border = Rectangle.BOX
                };
                PdfPTable whiteBoardTable = new PdfPTable(columns);
                whiteBoardTable.WidthPercentage = 80;
                whiteBoardTable.AddCell(whiteBoardCell);
                document.Add(whiteBoardTable);
                document.Add(new Paragraph("\n"));

                // Seating grid
                PdfPTable seatTable = new PdfPTable(columns);
                seatTable.WidthPercentage = 80;
                seatTable.SpacingBefore = 10f;
                seatTable.SpacingAfter = 10f;

                int seatIndex = 0;
                for (int r = 0; r < rows; r++)
                {
                    for (int c = 0; c < columns; c++)
                    {
                        if (seatIndex < shuffledSeatings.Count)
                        {
                            var s = shuffledSeatings[seatIndex];
                            var cell = new PdfPCell(new Phrase($"{s.SeatNumber}\n{s.Student.RollNumber}", FontFactory.GetFont(FontFactory.HELVETICA, 10)))
                            {
                                FixedHeight = 40f,
                                HorizontalAlignment = Element.ALIGN_CENTER,
                                VerticalAlignment = Element.ALIGN_MIDDLE,
                                Border = Rectangle.BOX
                            };
                            seatTable.AddCell(cell);
                        }
                        else if (seatIndex < roomCapacity)
                        {
                            // Empty seat
                            var cell = new PdfPCell(new Phrase("", FontFactory.GetFont(FontFactory.HELVETICA, 10)))
                            {
                                FixedHeight = 40f,
                                Border = Rectangle.BOX
                            };
                            seatTable.AddCell(cell);
                        }
                        seatIndex++;
                    }
                }
                document.Add(seatTable);
                document.Close();
                return File(ms.ToArray(), "application/pdf", "seating_plan.pdf");
            }
        }

        [HttpGet]
        [Route("api/Exam/attendance-sheet-pdf")]
        public async Task<IActionResult> GetAttendanceSheetPdf([FromQuery] int examId)
        {
            if (!IsExamAccessAllowed()) return AccessDenied();
            var examSeating = await _context.ExamSeatings
                .Include(e => e.Student)
                .Include(e => e.Course)
                .Include(e => e.Room)
                .FirstOrDefaultAsync(e => e.Id == examId);

            if (examSeating == null)
            {
                return NotFound("No seating arrangement found");
            }

            // Get all seatings for this exam (same course, room, and date)
            var examSeatings = await _context.ExamSeatings
                .Include(e => e.Student)
                .Where(e => e.CourseId == examSeating.CourseId && 
                           e.RoomId == examSeating.RoomId && 
                           e.ExamDate == examSeating.ExamDate)
                .ToListAsync();

            // Randomize the seating order
            var random = new Random();
            var randomizedSeatings = examSeatings.OrderBy(x => random.Next()).ToList();

            using (MemoryStream ms = new MemoryStream())
            {
                Document document = new Document(PageSize.A4, 25, 25, 30, 30);
                PdfWriter writer = PdfWriter.GetInstance(document, ms);
                document.Open();

                // Add title
                var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18);
                var title = new Paragraph($"Attendance Sheet - {examSeating.Course.Name}", titleFont)
                {
                    Alignment = Element.ALIGN_CENTER
                };
                document.Add(title);
                document.Add(new Paragraph($"Room: {examSeating.Room.RoomNumber}"));
                document.Add(new Paragraph($"Date: {examSeating.ExamDate:dd/MM/yyyy}"));
                document.Add(new Paragraph("\n"));

                // Create attendance table
                PdfPTable table = new PdfPTable(5); // Roll No, Name, Seat No, Signature, Sheet Code
                table.WidthPercentage = 100;
                table.SpacingBefore = 10f;
                table.SpacingAfter = 10f;
                table.SetWidths(new float[] { 2f, 4f, 2f, 3f, 2f }); // Adjust column widths

                // Add headers
                var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12);
                table.AddCell(new PdfPCell(new Phrase("Roll No", headerFont)) { Padding = 8 });
                table.AddCell(new PdfPCell(new Phrase("Name", headerFont)) { Padding = 8 });
                table.AddCell(new PdfPCell(new Phrase("Seat No", headerFont)) { Padding = 8 });
                table.AddCell(new PdfPCell(new Phrase("Signature", headerFont)) { Padding = 8 });
                table.AddCell(new PdfPCell(new Phrase("Sheet Code", headerFont)) { Padding = 8 });

                // Add student rows with more padding
                foreach (var seating in randomizedSeatings)
                {
                    table.AddCell(new PdfPCell(new Phrase(seating.Student.RollNumber)) { Padding = 8 });
                    table.AddCell(new PdfPCell(new Phrase(seating.Student.Name)) { Padding = 8 });
                    table.AddCell(new PdfPCell(new Phrase(seating.SeatNumber)) { Padding = 8 });
                    table.AddCell(new PdfPCell(new Phrase("")) { Padding = 8 }); // Empty cell for signature
                    table.AddCell(new PdfPCell(new Phrase("")) { Padding = 8 }); // Empty cell for sheet code
                }

                document.Add(table);
                document.Close();
                return File(ms.ToArray(), "application/pdf", "attendance_sheet.pdf");
            }
        }

        [HttpPost]
        [Route("api/Exam/delete-exam")]
        public async Task<IActionResult> DeleteExam([FromBody] int examId)
        {
            if (!IsExamAccessAllowed()) return AccessDenied();
            try
            {
                var examSeating = await _context.ExamSeatings
                    .FirstOrDefaultAsync(e => e.Id == examId);
                if (examSeating == null)
                {
                    return NotFound(new { success = false, message = "Exam arrangement not found." });
                }
                // Find all seatings for this exam (same course, room, and date)
                var examSeatings = await _context.ExamSeatings
                    .Where(e => e.CourseId == examSeating.CourseId &&
                                e.RoomId == examSeating.RoomId &&
                                e.ExamDate == examSeating.ExamDate)
                    .ToListAsync();
                _context.ExamSeatings.RemoveRange(examSeatings);
                await _context.SaveChangesAsync();
                return Ok(new { success = true, message = "Exam arrangement deleted successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }

    public class ExamArrangementRequest
    {
        public string Session { get; set; }
        public string Degree { get; set; }
        public string Section { get; set; }
        public int CourseId { get; set; }
        public int RoomId { get; set; }
        public DateTime ExamDate { get; set; }
        public TimeSpan ExamTime { get; set; }
        public int PaperTotalTime { get; set; } = 180; // Default 3 hours in minutes
    }
} 