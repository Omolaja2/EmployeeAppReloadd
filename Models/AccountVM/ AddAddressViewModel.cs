using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Presentation.Models.AccountVM
{
    public class  AddAddressViewModel
    {
        
        [Required]
        public Guid EmployeeId { get; set; }

        [Required]
        public string Street { get; set; } = default!;

        [Required]
        public string City { get; set; } = default!;

        [Required]
        public string ZipCode { get; set; } = default!;

        [Required]
        public Guid StateId { get; set; }

        public List<SelectListItem> States { get; set; } = new();
    }
}