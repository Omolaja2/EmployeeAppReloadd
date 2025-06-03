using System;
using System.ComponentModel.DataAnnotations;

namespace Presentation.Models;

public class DetaildepartmentViewModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string? Description { get; set; }    
}
