using System;
using System.ComponentModel.DataAnnotations;

namespace Presentation.Models.AccountVM;

public class Verify2faViewModel
{
    [Required]
    public string Code { get; set; } = default!;

    public bool RememberMe { get; set; }
}
