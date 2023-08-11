using System.ComponentModel.DataAnnotations;

namespace SharadDemoProject.Model.Employees
{
    public class EmployeeModel
    {
        [Key]
        public int EmpId { get; set; }

        [Required]
        [StringLength(50, MinimumLength = 2)]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Only string values and one space are allowed.")]
        public string? EmpName { get; set; }

        [Required]
        public string? EmpCity { get; set; }

        [EmailAddress(ErrorMessage = "Invalid email address.")]
        [RegularExpression(@"^.+@.+\.com$", ErrorMessage = "Please enter a valid email")]
        public string? EmpEmail { get; set; }

        [Required]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Please enter a 10-digit phone number.")]
        public string? EmpPhone { get; set; }
    }
}
