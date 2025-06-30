using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Presentation.Models
{
    public class CreateEmployeeViewModel
    {
        public Guid Id { get; set; }

        [Required]
        public string FirstName { get; set; } = default!;

        [Required]
        public string LastName { get; set; } = default!;

        [Required, EmailAddress]
        public string Email { get; set; } = default!;

        [Required]
        public DateTime HireDate { get; set; }

        [Required]
        public decimal Salary { get; set; }

        [Required]
        public Guid DepartmentId { get; set; }

        public IFormFile? ProfileImage { get; set; }

        public List<SelectListItem>? Departments { get; set; }

        
        public string? ProfileImageUrl { get; set; }

        public string? ProfileImagePublicId { get; set; }


        public string? DepartmentName { get; set; }
    }
}
