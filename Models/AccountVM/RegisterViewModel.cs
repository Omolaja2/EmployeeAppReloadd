using System;
using System.ComponentModel.DataAnnotations;

namespace Presentation.Models.AccountVM;

public class RegisterViewModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = default!;

    [Required]
    public string Password { get; set; } = default!;

    [Required]
    [Compare("Password")]
    public string ConfirmPassword { get; set; } = default!;
}
