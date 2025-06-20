using System;
using System.ComponentModel.DataAnnotations;

namespace Presentation.Models.AccountVM;

public class LoginViewModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = default!;

    [Required]
    public string Password { get; set; } = default!;

    public bool RememberMe { get; set; } = false;
}
