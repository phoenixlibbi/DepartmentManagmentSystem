using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MS.Models
{
    public class ExamSeating
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int RoomId { get; set; }

        [Required]
        public int CourseId { get; set; }

        [Required]
        public int StudentId { get; set; }

        [Required]
        public string SeatNumber { get; set; }

        public DateTime ExamDate { get; set; }

        [Required]
        public TimeSpan ExamTime { get; set; }

        [Required]
        public int PaperTotalTime { get; set; } = 180; // Default 3 hours in minutes

        public bool IsPresent { get; set; }

        // Navigation properties
        [ForeignKey("RoomId")]
        public virtual Room Room { get; set; }

        [ForeignKey("CourseId")]
        public virtual Course Course { get; set; }

        [ForeignKey("StudentId")]
        public virtual Student Student { get; set; }
    }
} 