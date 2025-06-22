using System;
using System.ComponentModel.DataAnnotations;

namespace Presentation.Models.AccountVM;

public class ForgotPasswordViewModel
{
    [Required]
    public string Email { get; set; } = default!;
}
