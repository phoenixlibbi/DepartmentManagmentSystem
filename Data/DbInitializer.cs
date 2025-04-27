using MS.Models;

namespace MS.Data
{
    public static class DbInitializer
    {
        public static void Initialize(ApplicationDbContext context)
        {
            // Delete and recreate the database
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            // Add sample courses
            var courses = new Course[]
            {
                new Course { Code = "CS101", Name = "Introduction to Programming", CreditHours = 3 },
                new Course { Code = "CS201", Name = "Data Structures", CreditHours = 4 },
                new Course { Code = "CS301", Name = "Database Systems", CreditHours = 3 },
                new Course { Code = "MTH101", Name = "Calculus I", CreditHours = 3 }
            };
            context.Courses.AddRange(courses);
            context.SaveChanges();

            // Add sample rooms
            var rooms = new Room[]
            {
                new Room { RoomNumber = "R-101", Capacity = 40, IsBooked = false },
                new Room { RoomNumber = "R-102", Capacity = 35, IsBooked = false },
                new Room { RoomNumber = "R-103", Capacity = 50, IsBooked = false },
                new Room { RoomNumber = "R-104", Capacity = 30, IsBooked = false }
            };
            context.Rooms.AddRange(rooms);
            context.SaveChanges();

            // Add sample students
            var students = new Student[]
            {
                new Student {
                    Name = "John Doe",
                    RollNumber = "2023-CS-01",
                    Phone = "1234567890",
                    Email = "john@example.com",
                    Address = "123 Main St",
                    Age = 20,
                    Gender = "Male",
                    CNIC = "12345-1234567-1",
                    Session = "2023",
                    Degree = "CS"
                },
                new Student {
                    Name = "Jane Smith",
                    RollNumber = "2023-CS-02",
                    Phone = "0987654321",
                    Email = "jane@example.com",
                    Address = "456 Oak St",
                    Age = 19,
                    Gender = "Female",
                    CNIC = "12345-7654321-2",
                    Session = "2023",
                    Degree = "CS"
                },
                new Student {
                    Name = "Alice Johnson",
                    RollNumber = "2023-SE-01",
                    Phone = "5555555555",
                    Email = "alice@example.com",
                    Address = "789 Pine St",
                    Age = 21,
                    Gender = "Female",
                    CNIC = "12345-9876543-3",
                    Session = "2023",
                    Degree = "SE"
                }
            };
            context.Students.AddRange(students);
            context.SaveChanges();

            // Add sample exam seating
            var examSeating = new ExamSeating
            {
                RoomId = rooms[0].Id,
                CourseId = courses[0].Id,
                StudentId = students[0].Id,
                SeatNumber = "R1S1",
                ExamDate = DateTime.Now.AddDays(7),
                IsPresent = false
            };
            context.ExamSeatings.Add(examSeating);
            context.SaveChanges();
        }
    }
} 