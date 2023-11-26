using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mark2API.Models
{
    public class User
    {
        [Key]
        public int Reg_Id { get; set; }

        [Required(ErrorMessage = "Full name is required.")]
        [StringLength(50, ErrorMessage = "Full name must be less than or equal to 50 characters.")]
        public string? FullName { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        [RegularExpression(@"^(?=.*\d)(?=.*[a-z])(?=.*[A-Z])(?=.*[@#$%^&+=]).{8,}$",
        ErrorMessage = "Password must contain at least 1 uppercase, 1 lowercase, 1 special character, 1 number, and be at least 8 characters long.")]
        public string? Password { get; set; }

        [NotMapped]
        [Required(ErrorMessage = "Confirm password is required.")]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        [DataType(DataType.Password)]
        public string? ConfirmPassword { get; set; }

        [Required(ErrorMessage = "Phone number is required.")]
        [RegularExpression(@"^[789]\d{9}$", ErrorMessage = "Invalid phone number. It must be 10 digits and start with 7, 8, or 9.")]
        [StringLength(10, MinimumLength = 10, ErrorMessage = "Phone number must be exactly 10 digits.")]
        public string? Mobile { get; set; }

        [Required(ErrorMessage = "City is required.")]
        public string? City { get; set; }

        [Required(ErrorMessage = "Date of birth is required.")]
        [DataType(DataType.Date)]
        [Column(TypeName = "Date")]
        public DateTime DateOfBirth { get; set; }

        [Required(ErrorMessage = "State is required.")]
        public string? State { get; set; }

        [Required(ErrorMessage = "Qualification is required.")]
        public string? Qualification { get; set; }

        [Required(ErrorMessage = "Year of completion is required.")]
        [Range(1980, 2035, ErrorMessage = "Invalid Year Range must be 1980 to 2035")]
        [RegularExpression(@"^\d{4}$", ErrorMessage = "Year of completion must be 4 digits.")]
        [StringLength(4, MinimumLength = 4, ErrorMessage = "Year of completion must be 4 digits.")]
        public string? YearOfCompletion { get; set; }

        public class LoginRequest
        {
            public string? Email { get; set; }
            public string? Password { get; set; }
        }
        public class PasswordReset
        {
            public string? Email { get; set; }

            // Add a ResetToken property
            public string? ResetToken { get; set; }
        }
        public class ConfirmReset
        {
            public string? Email { get; set; }

            // Add a ResetToken property
            public string? ResetToken { get; set; }

            [Required(ErrorMessage = "Password is required.")]
            [DataType(DataType.Password)]
            [RegularExpression(@"^(?=.*\d)(?=.*[a-z])(?=.*[A-Z])(?=.*[@#$%^&+=]).{8,}$",
         ErrorMessage = "Password must contain at least 1 uppercase, 1 lowercase, 1 special character, 1 number, and be at least 8 characters long.")]
            public string? Password { get; set; }

            [Required(ErrorMessage = "Confirm password is required.")]
            [Compare("Password", ErrorMessage = "Passwords do not match.")]
            [DataType(DataType.Password)]
            public string? ConfirmPassword { get; set; }
        }

    }
}
