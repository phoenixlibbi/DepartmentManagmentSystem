using Microsoft.EntityFrameworkCore;
using MS.Models;

namespace MS.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Student> Students { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<ExamSeating> ExamSeatings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Student RollNumber generation
            modelBuilder.Entity<Student>()
                .Property(s => s.RollNumber)
                .IsRequired();

            // Configure relationships
            modelBuilder.Entity<ExamSeating>()
                .HasOne(e => e.Student)
                .WithMany(s => s.ExamSeatings)
                .HasForeignKey(e => e.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ExamSeating>()
                .HasOne(e => e.Course)
                .WithMany(c => c.ExamSeatings)
                .HasForeignKey(e => e.CourseId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ExamSeating>()
                .HasOne(e => e.Room)
                .WithMany(r => r.ExamSeatings)
                .HasForeignKey(e => e.RoomId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
} 