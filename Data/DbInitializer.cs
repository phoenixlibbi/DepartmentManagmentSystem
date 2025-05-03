using MS.Models;
using Microsoft.EntityFrameworkCore;

namespace MS.Data
{
    public static class DbInitializer
    {
        public static void Initialize(ApplicationDbContext context)
        {
            // Apply any pending migrations
            context.Database.Migrate();

            // Check if any data exists
            if (context.Courses.Any() || context.Rooms.Any() || context.Students.Any())
            {
                return; // Database has been seeded
            }

            // Add sample courses
            var courses = new Course[]
            {
                new Course { Code = "CS101", Name = "Introduction to Programming", CreditHours = 3, Degree = "CS" },
                new Course { Code = "CS201", Name = "Data Structures", CreditHours = 4, Degree = "CS" },
                new Course { Code = "CS301", Name = "Database Systems", CreditHours = 3, Degree = "CS" },
                new Course { Code = "SE101", Name = "Software Engineering", CreditHours = 3, Degree = "SE" },
                new Course { Code = "SE201", Name = "Software Design", CreditHours = 4, Degree = "SE" }
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
                new Student
                {
                    Name = "John Doe",
                    Session = "2023",
                    Degree = "CS",
                    Phone = "1234567890",
                    Email = "john@example.com",
                    Address = "123 Main St",
                    Age = 20,
                    Gender = "Male",
                    CNIC = "12345-6789012-3",
                    RollNumber = "CS-2023-001"
                },
                new Student
                {
                    Name = "Jane Smith",
                    Session = "2023",
                    Degree = "CS",
                    Phone = "0987654321",
                    Email = "jane@example.com",
                    Address = "456 Oak St",
                    Age = 21,
                    Gender = "Female",
                    CNIC = "98765-4321098-7",
                    RollNumber = "CS-2023-002"
                },
                new Student
                {
                    Name = "Bob Johnson",
                    Session = "2024",
                    Degree = "SE",
                    Phone = "5551234567",
                    Email = "bob@example.com",
                    Address = "789 Pine St",
                    Age = 19,
                    Gender = "Male",
                    CNIC = "45678-9012345-6",
                    RollNumber = "SE-2024-001"
                }
            };
            context.Students.AddRange(students);
            context.SaveChanges();
        }
    }
} 