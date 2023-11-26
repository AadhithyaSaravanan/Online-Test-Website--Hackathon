using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mark2MVC.Models
{
    public class Course
    {
        [Key]
        public int CourseId { get; set; }
        
        [Required(ErrorMessage = "Name is required")]
        [StringLength(255, ErrorMessage = "Name can have a maximum of 255 characters")]
        public string? CourseName { get; set; }

        [Required(ErrorMessage = "Description is required")]
        [StringLength(1000, ErrorMessage = "Description can have a maximum of 1000 characters")]
        public string? Description { get; set; }

        public byte[]? Image { get; set; }

        public ICollection<Question>? Questions { get; set; }

    }
}