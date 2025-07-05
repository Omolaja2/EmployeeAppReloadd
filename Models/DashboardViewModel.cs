using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Presentation.Models
{
    public class DashboardViewModel
    {
        public int TotalEmployees { get; set; }

        public List<DepartmentCount> DepartmentDistribution { get; set; } = new();

        public List<MonthlyHire> MonthlyHires { get; set; } = new();
    }

    public class DepartmentCount
    {
        public string DepartmentName { get; set; } = default!;
        public int Count { get; set; }
    }

    public class MonthlyHire
    {
        public string Month { get; set; } = default!;
        public int Count { get; set; }
    }

}