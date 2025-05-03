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
        [Route("api/Exam/sessions")]
        public IActionResult GetSessions()
        {
            var currentYear = DateTime.Now.Year;
            var years = Enumerable.Range(2000, currentYear - 2000 + 1)
                .Select(y => new { value = y.ToString(), text = y.ToString() })
                .ToList();
            return Json(years);
        }

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
                // Validate room capacity and availability
                var room = await _context.Rooms.FindAsync(request.RoomId);
                if (room == null)
                {
                    return BadRequest("Room not found");
                }

                if (room.IsBooked)
                {
                    return BadRequest("Room is already booked for another exam");
                }

                var students = await _context.Students
                    .Where(s => s.Session == request.Session && s.Degree == request.Degree)
                    .ToListAsync();

                if (students.Count > room.Capacity)
                {
                    return BadRequest("Room capacity is less than number of students");
                }

                // Mark room as booked
                room.IsBooked = true;
                _context.Rooms.Update(room);

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
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [Route("api/Exam/seating-plan-pdf")]
        public async Task<IActionResult> GetSeatingPlanPdf([FromQuery] int examId)
        {
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

            // Use the same seat order as the seating plan
            var seatOrder = examSeatings.OrderBy(e => e.SeatNumber).ToList();

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
                PdfPTable table = new PdfPTable(5); // Roll No, Name, Seat No, Signature, Remarks
                table.WidthPercentage = 100;
                table.SpacingBefore = 10f;
                table.SpacingAfter = 10f;

                // Add headers
                var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12);
                table.AddCell(new PdfPCell(new Phrase("Roll No", headerFont)));
                table.AddCell(new PdfPCell(new Phrase("Name", headerFont)));
                table.AddCell(new PdfPCell(new Phrase("Seat No", headerFont)));
                table.AddCell(new PdfPCell(new Phrase("Signature", headerFont)));
                table.AddCell(new PdfPCell(new Phrase("Remarks", headerFont)));

                foreach (var seating in seatOrder)
                {
                    table.AddCell(seating.Student.RollNumber);
                    table.AddCell(seating.Student.Name);
                    table.AddCell(seating.SeatNumber);
                    table.AddCell(""); // Empty cell for signature
                    table.AddCell(""); // Empty cell for remarks
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