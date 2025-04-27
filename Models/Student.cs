using System.ComponentModel.DataAnnotations;

namespace MS.Models
{
    public class Student
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        public string RollNumber { get; set; }

        [Required]
        [Phone]
        public string Phone { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Address { get; set; }

        [Required]
        [Range(16, 100)]
        public int Age { get; set; }

        [Required]
        public string Gender { get; set; }

        [Required]
        [StringLength(15)]
        public string CNIC { get; set; }

        [Required]
        public string Session { get; set; }

        [Required]
        public string Degree { get; set; }

        // Navigation property for exams
        public virtual ICollection<ExamSeating> ExamSeatings { get; set; }
    }
} 