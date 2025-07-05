using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Data.Context;
using Presentation.Models; 

namespace Presentation.Controllers
{
    public class DashboardController : Controller
    {
        private readonly EmployeeAppDbContext _context;

        public DashboardController(EmployeeAppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var totalEmployees = await _context.Employees.CountAsync();

            var departmentDistribution = await _context.Employees
                .Include(e => e.Department)
                .GroupBy(e => e.Department!.Name)
                .Select(g => new DepartmentCount
                {
                    DepartmentName = g.Key,
                    Count = g.Count()
                })
                .ToListAsync();

            var monthlyHires = await _context.Employees
                .GroupBy(e => e.HireDate.Month)
                .Select(g => new MonthlyHire
                {
                    Month = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(g.Key),
                    Count = g.Count()
                })
                .ToListAsync();

            var viewModel = new DashboardViewModel
            {
                TotalEmployees = totalEmployees,
                DepartmentDistribution = departmentDistribution,
                MonthlyHires = monthlyHires
            };

            return View(viewModel);
        }
    }
}
