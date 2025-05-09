using System.ComponentModel.DataAnnotations;

namespace MS.Models
{
    public class Course
    {
        public Course()
        {
            ExamSeatings = new List<ExamSeating>();
        }

        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [StringLength(20)]
        public string Code { get; set; }

        [Required]
        [Range(1, 6)]
        public int CreditHours { get; set; }

        [Required]
        [StringLength(10)]
        public string Degree { get; set; }

        // Navigation property for exams
        public virtual ICollection<ExamSeating>? ExamSeatings { get; set; }
    }
} 