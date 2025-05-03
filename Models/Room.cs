using System.ComponentModel.DataAnnotations;

namespace MS.Models
{
    public class Room
    {
        public Room()
        {
            ExamSeatings = new List<ExamSeating>();
        }

        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        public string RoomNumber { get; set; }

        [Required]
        [Range(1, 500)]
        public int Capacity { get; set; }

        public bool IsBooked { get; set; }

        // Navigation property for exams
        public virtual ICollection<ExamSeating>? ExamSeatings { get; set; }
    }
} 