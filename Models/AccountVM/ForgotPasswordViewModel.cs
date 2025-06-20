using System;
using System.ComponentModel.DataAnnotations;

namespace Presentation.Models.AccountVM;

public class ForgotPasswordViewModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = default!;

    [Required]
    public string NewPassword { get; set; } = default!;

    [Required]
    [Compare("NewPassword")]
    public string ConfirmPassword { get; set; } = default!;
}
