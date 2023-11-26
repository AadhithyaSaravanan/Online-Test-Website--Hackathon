using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mark2API.Models
{
    public class TestResultReport
    {

        [Key]
        public int Id { get; set; }


        [Required]
        [ForeignKey("User")]
        public int Reg_Id { get; set; } // Foreign key referencing the User table

        [Required]
        public string? FullName { get; set; }

        [Required]
        [EmailAddress]
        public string? Email { get; set; }

        [Required]
        [Phone]
        public string? Mobile { get; set; }

        [Required]
        public string? State { get; set; }

        [Required]
        public string? City { get; set; }

        [Required]
        public string? CourseName { get; set; }

        [Required]
        public int TotalMarks { get; set; }

        [Required]
        public string? Level { get; set; }

        public User? User { get; set; } // Navigation property for the related User model
    }
}

