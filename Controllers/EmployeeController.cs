using Data.Context;
using Data.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Presentation.Models;

namespace Presentation.Controllers
{

    public class EmployeeController : Controller
    {
        private readonly EmployeeAppDbContext _context;

        public EmployeeController(EmployeeAppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var employees = await _context.Employees.Include(e => e.Department).ToListAsync();

            var model = new ListEmployeeViewModel
            {
                Employee = employees.Select(e => new CreateEmployeeViewModel
                {
                    Id = e.Id,
                    FirstName = e.FirstName,
                    LastName = e.LastName,
                    Email = e.Email,
                    HireDate = e.HireDate,
                    Salary = e.Salary,
                    DepartmentId = e.DepartmentId,
                    DepartmentName = e.Department?.Name
                }).ToList()
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var departments = await _context.Departments.ToListAsync();

            var model = new CreateEmployeeViewModel
            {
                Departments = departments.Select(d => new SelectListItem
                {
                    Value = d.Id.ToString(),
                    Text = d.Name
                }).ToList()
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateEmployeeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Departments = await _context.Departments
                    .Select(d => new SelectListItem { Value = d.Id.ToString(), Text = d.Name })
                    .ToListAsync();
                return View(model);
            }

            var employee = new Employee
            {
                Id = Guid.NewGuid(),
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                HireDate = model.HireDate,
                Salary = model.Salary,
                DepartmentId = model.DepartmentId
            };

            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Detail(Guid id)
        {
            var employee = await _context.Employees
                .Include(e => e.Department)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (employee == null)
                return NotFound();

            return View(employee);
        }

        public async Task<IActionResult> Update(Guid id)
        {
            var employee = await _context.Employees.FirstOrDefaultAsync(c => c.Id == id);
            if (employee == null) return NotFound();

            var updateVm = new EditEmployeeViewModel
            {
                Id = employee.Id,
                FirstName = employee.FirstName,
                LastName = employee.LastName,
                Email = employee.Email,
                HireDate = employee.HireDate,
                Salary = employee.Salary,
                DepartmentId = employee.DepartmentId,
                Departments = await _context.Departments
                    .Select(d => new SelectListItem { Value = d.Id.ToString(), Text = d.Name })
                    .ToListAsync()
            };

            return View(updateVm);
        }

        [HttpPost]
        public async Task<IActionResult> Update(EditEmployeeViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                viewModel.Departments = await _context.Departments
                    .Select(d => new SelectListItem { Value = d.Id.ToString(), Text = d.Name })
                    .ToListAsync();
                return View(viewModel);
            }

            var employee = await _context.Employees.FindAsync(viewModel.Id);
            if (employee == null) return NotFound();

            employee.FirstName = viewModel.FirstName;
            employee.LastName = viewModel.LastName;
            employee.Email = viewModel.Email;
            employee.HireDate = viewModel.HireDate;
            employee.Salary = viewModel.Salary;
            employee.DepartmentId = viewModel.DepartmentId;

            _context.Employees.Update(employee);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Delete(Guid id)
        {
            var employee = await _context.Employees
                .Include(e => e.Department)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (employee == null) return NotFound();

            return View(employee);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee == null) return NotFound();

            _context.Employees.Remove(employee);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }
    }
}

