using MS.Models;
using Microsoft.EntityFrameworkCore;

namespace MS.Data
{
    public static class DbInitializer
    {
        public static void Initialize(ApplicationDbContext context)
        {
            try
            {
                // Check if database exists
                if (!context.Database.CanConnect())
                {
                    Console.WriteLine("Database does not exist. Creating database...");
                    context.Database.EnsureCreated();
                }

                // Check if we already have data
                if (context.Courses.Any())
                {
                    Console.WriteLine("Database already contains data. Skipping seeding.");
                return; // Database has been seeded
            }

                Console.WriteLine("Starting database seeding...");

            // Add sample courses
            var courses = new Course[]
            {
                    new Course { Name = "Introduction to Programming", Code = "CS101", CreditHours = 3, Degree = "BS" },
                    new Course { Name = "Database Systems", Code = "CS201", CreditHours = 3, Degree = "BS" },
                    new Course { Name = "Web Development", Code = "CS301", CreditHours = 3, Degree = "BS" },
                    new Course { Name = "Data Structures", Code = "CS401", CreditHours = 3, Degree = "BS" },
                    new Course { Name = "Software Engineering", Code = "CS501", CreditHours = 3, Degree = "BS" }
            };

            context.Courses.AddRange(courses);
            context.SaveChanges();
                Console.WriteLine("Courses seeded successfully.");

            // Add sample rooms
            var rooms = new Room[]
            {
                    new Room { RoomNumber = "101", Capacity = 30, IsBooked = false },
                    new Room { RoomNumber = "102", Capacity = 25, IsBooked = false },
                    new Room { RoomNumber = "103", Capacity = 35, IsBooked = false },
                    new Room { RoomNumber = "201", Capacity = 40, IsBooked = false },
                    new Room { RoomNumber = "202", Capacity = 30, IsBooked = false }
            };

            context.Rooms.AddRange(rooms);
            context.SaveChanges();
                Console.WriteLine("Rooms seeded successfully.");

            // Add sample students
            var students = new Student[]
            {
                    new Student { 
                    Name = "John Doe",
                    Session = "2023",
                        Degree = "BS", 
                    Phone = "1234567890",
                    Email = "john@example.com",
                    Address = "123 Main St",
                    Age = 20,
                    Gender = "Male",
                    CNIC = "12345-6789012-3",
                        RollNumber = "BS2023001" 
                },
                    new Student { 
                    Name = "Jane Smith",
                    Session = "2023",
                        Degree = "BS", 
                        Phone = "2345678901", 
                    Email = "jane@example.com",
                    Address = "456 Oak St",
                    Age = 21,
                    Gender = "Female",
                        CNIC = "23456-7890123-4", 
                        RollNumber = "BS2023002" 
                }
            };

            context.Students.AddRange(students);
            context.SaveChanges();
                Console.WriteLine("Students seeded successfully.");

                // Add sample users
                var users = new User[]
                {
                    new User { Username = "admin", Password = "admin123", Role = "SUPER_ADMIN" },
                    new User { Username = "clerk1", Password = "clerk123", Role = "CLERK" },
                    new User { Username = "clerk2", Password = "clerk456", Role = "CLERK" }
                };

                context.Users.AddRange(users);
                context.SaveChanges();
                Console.WriteLine("Users seeded successfully.");
                Console.WriteLine("Database seeding completed successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while seeding the database: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }
    }
} 