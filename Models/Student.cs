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
        [StringLength(4)]
        public string Session { get; set; }

        [Required]
        [StringLength(10)]
        public string Degree { get; set; }

        [Required]
        [Phone]
        [StringLength(15)]
        public string Phone { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; }

        [Required]
        [StringLength(200)]
        public string Address { get; set; }

        [Required]
        [Range(16, 100)]
        public int Age { get; set; }

        [Required]
        [StringLength(10)]
        public string Gender { get; set; }

        [Required]
        [StringLength(15)]
        public string CNIC { get; set; }

        [StringLength(20)]
        public string? RollNumber { get; set; }

        public virtual ICollection<ExamSeating>? ExamSeatings { get; set; }
    }
} 