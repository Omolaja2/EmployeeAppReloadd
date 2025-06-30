using System;
using System.ComponentModel.DataAnnotations;

namespace Presentation.Models.AccountVM;

public class EditProfileViewModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = default!;
}
