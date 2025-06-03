namespace Presentation.Models
{
    public class DepartmentViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = default!;
        public string? Description { get; set; }

        public List<DepartmentViewModel> Departments { get; set; } = new();
    }
}
