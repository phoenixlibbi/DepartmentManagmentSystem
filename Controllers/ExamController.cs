using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MS.Data;
using MS.Models;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.Text;

namespace MS.Controllers
{
    public class ExamController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ExamController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Exam
        public IActionResult Index()
        {
            return View();
        }

        // API Endpoints
        [Route("api/Exam/courses")]
        public async Task<IActionResult> GetCourses([FromQuery] string degree)
        {
            var courses = await _context.Courses
                .Where(c => c.Code.StartsWith(degree))
                .Select(c => new { id = c.Id, code = c.Code, name = c.Name })
                .ToListAsync();

            return Json(courses);
        }

        [Route("api/Exam/rooms")]
        public async Task<IActionResult> GetRooms()
        {
            var rooms = await _context.Rooms
                .Where(r => !r.IsBooked)
                .Select(r => new { id = r.Id, roomNumber = r.RoomNumber, capacity = r.Capacity })
                .ToListAsync();

            return Json(rooms);
        }

        [Route("api/Exam/list")]
        public async Task<IActionResult> GetExamList()
        {
            var exams = await _context.ExamSeatings
                .Include(e => e.Course)
                .Include(e => e.Room)
                .GroupBy(e => new { e.CourseId, e.RoomId, e.ExamDate })
                .Select(g => new
                {
                    id = g.First().Id,
                    courseCode = g.First().Course.Code,
                    courseName = g.First().Course.Name,
                    roomNumber = g.First().Room.RoomNumber,
                    examDate = g.First().ExamDate,
                    studentCount = g.Count()
                })
                .OrderByDescending(e => e.examDate)
                .ToListAsync();

            return Json(exams);
        }

        [HttpPost]
        [Route("api/Exam/arrange-seating")]
        public async Task<IActionResult> ArrangeSeating([FromBody] ExamArrangementRequest request)
        {
            try
            {
                // Validate room capacity
                var room = await _context.Rooms.FindAsync(request.RoomId);
                if (room == null)
                {
                    return BadRequest("Room not found");
                }

                var students = await _context.Students
                    .Where(s => s.Session == request.Session && s.Degree == request.Degree)
                    .ToListAsync();

                if (students.Count > room.Capacity)
                {
                    return BadRequest("Room capacity is less than number of students");
                }

                // Create random seating arrangement
                var random = new Random();
                var shuffledStudents = students.OrderBy(x => random.Next()).ToList();
                var examSeatings = new List<ExamSeating>();

                for (int i = 0; i < shuffledStudents.Count; i++)
                {
                    var seating = new ExamSeating
                    {
                        RoomId = room.Id,
                        CourseId = request.CourseId,
                        StudentId = shuffledStudents[i].Id,
                        SeatNumber = $"R{i / 4 + 1}S{i % 4 + 1}", // 4 seats per row
                        ExamDate = request.ExamDate,
                        IsPresent = false
                    };
                    examSeatings.Add(seating);
                }

                _context.ExamSeatings.AddRange(examSeatings);
                await _context.SaveChangesAsync();

                return Ok(true);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        [Route("api/Exam/seating-plan-pdf")]
        public async Task<IActionResult> GetSeatingPlanPdf([FromQuery] int examId)
        {
            var examSeatings = await _context.ExamSeatings
                .Include(e => e.Student)
                .Include(e => e.Course)
                .Include(e => e.Room)
                .Where(e => e.Id == examId)
                .OrderBy(e => e.SeatNumber)
                .ToListAsync();

            if (!examSeatings.Any())
            {
                return NotFound("No seating arrangement found");
            }

            using (MemoryStream ms = new MemoryStream())
            {
                Document document = new Document(PageSize.A4, 25, 25, 30, 30);
                PdfWriter writer = PdfWriter.GetInstance(document, ms);
                document.Open();

                // Add title
                var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18);
                var title = new Paragraph($"Seating Plan - {examSeatings[0].Course.Name}", titleFont);
                title.Alignment = Element.ALIGN_CENTER;
                document.Add(title);
                document.Add(new Paragraph($"Room: {examSeatings[0].Room.RoomNumber}"));
                document.Add(new Paragraph($"Date: {examSeatings[0].ExamDate:dd/MM/yyyy}"));
                document.Add(new Paragraph("\n"));

                // Create seating table
                PdfPTable table = new PdfPTable(4); // 4 columns
                table.WidthPercentage = 100;

                foreach (var seating in examSeatings)
                {
                    var cell = new PdfPCell(new Phrase(
                        $"Seat: {seating.SeatNumber}\n" +
                        $"Roll No: {seating.Student.RollNumber}\n" +
                        $"Name: {seating.Student.Name}"
                    ));
                    cell.Padding = 5;
                    table.AddCell(cell);
                }

                document.Add(table);
                document.Close();

                return File(ms.ToArray(), "application/pdf", "seating_plan.pdf");
            }
        }

        [HttpGet]
        [Route("api/Exam/attendance-sheet-pdf")]
        public async Task<IActionResult> GetAttendanceSheetPdf([FromQuery] int examId)
        {
            var examSeatings = await _context.ExamSeatings
                .Include(e => e.Student)
                .Include(e => e.Course)
                .Include(e => e.Room)
                .Where(e => e.Id == examId)
                .OrderBy(e => e.Student.RollNumber)
                .ToListAsync();

            if (!examSeatings.Any())
            {
                return NotFound("No seating arrangement found");
            }

            using (MemoryStream ms = new MemoryStream())
            {
                Document document = new Document(PageSize.A4, 25, 25, 30, 30);
                PdfWriter writer = PdfWriter.GetInstance(document, ms);
                document.Open();

                // Add title
                var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18);
                var title = new Paragraph($"Attendance Sheet - {examSeatings[0].Course.Name}", titleFont);
                title.Alignment = Element.ALIGN_CENTER;
                document.Add(title);
                document.Add(new Paragraph($"Room: {examSeatings[0].Room.RoomNumber}"));
                document.Add(new Paragraph($"Date: {examSeatings[0].ExamDate:dd/MM/yyyy}"));
                document.Add(new Paragraph("\n"));

                // Create attendance table
                PdfPTable table = new PdfPTable(4); // Roll No, Name, Seat No, Signature
                table.WidthPercentage = 100;

                // Add headers
                table.AddCell("Roll No");
                table.AddCell("Name");
                table.AddCell("Seat No");
                table.AddCell("Signature");

                foreach (var seating in examSeatings)
                {
                    table.AddCell(seating.Student.RollNumber);
                    table.AddCell(seating.Student.Name);
                    table.AddCell(seating.SeatNumber);
                    table.AddCell(""); // Empty cell for signature
                }

                document.Add(table);
                document.Close();

                return File(ms.ToArray(), "application/pdf", "attendance_sheet.pdf");
            }
        }
    }

    public class ExamArrangementRequest
    {
        public string Session { get; set; }
        public string Degree { get; set; }
        public int CourseId { get; set; }
        public int RoomId { get; set; }
        public DateTime ExamDate { get; set; }
    }
} 