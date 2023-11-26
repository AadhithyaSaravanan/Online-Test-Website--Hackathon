using Mark2API.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mark2API.Models
{
    public class Question
    {
        [Key]
        public int QuestionId { get; set; }

        [Required(ErrorMessage = "CourseId is required")]
        [Range(1, int.MaxValue, ErrorMessage = "CourseId must be a positive integer")]
        public int CourseId { get; set; }

        [ForeignKey("CourseId")] // Define the foreign key relationship
        public Course? Course { get; set; } // Navigation property to Course

        [Required(ErrorMessage = "QuestionText is required")]
        [StringLength(1000, ErrorMessage = "QuestionText can have a maximum of 1000 characters")]
        public string? Questions { get; set; }

        [Required(ErrorMessage = "Option1 is required")]
        [StringLength(255, ErrorMessage = "Option1 can have a maximum of 255 characters")]
        public string? Option1 { get; set; }

        [Required(ErrorMessage = "Option2 is required")]
        [StringLength(255, ErrorMessage = "Option2 can have a maximum of 255 characters")]
        public string? Option2 { get; set; }

        [Required(ErrorMessage = "Option3 is required")]
        [StringLength(255, ErrorMessage = "Option3 can have a maximum of 255 characters")]
        public string? Option3 { get; set; }

        [Required(ErrorMessage = "Option4 is required")]
        [StringLength(255, ErrorMessage = "Option4 can have a maximum of 255 characters")]
        public string? Option4 { get; set; }

        [Required(ErrorMessage = "CorrectAnswer is required")]
        [StringLength(255, ErrorMessage = "CorrectAnswer can have a maximum of 255 characters")]
        public string? CorrectAnswer { get; set; }

        public string? Level { get; set; }
    }
}