using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Presentation.Models.AccountVM
{
    public class CreateAddressViewModel
    {
        public Guid EmployeeId { get; set; }

        [Required]
        public string Street { get; set; } = default!;

        [Required]
        public string City { get; set; } = default!;

        [Required]
        public string State { get; set; } = default!;

        public string ZipCode { get; set; } = default!;

        public List<SelectListItem>? States { get; set; }   
    }
}
