using System;
using System.ComponentModel.DataAnnotations;

namespace Presentation.Models.AccountVM;

public class ResetPasswordViewModel
{
    [Required]
    public string Email { get; set; } = default!;

        [Required]
        public string Token { get; set; } = default!;

        [Required]
        public string NewPassword { get; set; } = default!;

        [Required]
        [Compare("NewPassword", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; } = default!;
}
