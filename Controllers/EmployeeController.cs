using Data.Context;
using Data.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Presentation.Models;
using Application.Services.UploadImage;

namespace Presentation.Controllers
{
    public class EmployeeController : Controller
    {
        private readonly EmployeeAppDbContext _context;
        private readonly ICloudinaryService _cloudinaryService;

        public EmployeeController(EmployeeAppDbContext context, ICloudinaryService cloudinaryService)
        {
            _context = context;
            _cloudinaryService = cloudinaryService;
        }

        private async Task<List<SelectListItem>> GetDepartmentsAsync()
        {
            return await _context.Departments
                .Select(d => new SelectListItem
                {
                    Value = d.Id.ToString(),
                    Text = d.Name
                }).ToListAsync();
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
                    ProfileImageUrl = e.ProfileImageUrl
                }).ToList()
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = new CreateEmployeeViewModel
            {
                Departments = await GetDepartmentsAsync()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateEmployeeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Departments = await GetDepartmentsAsync();
                return View(model);
            }

            string? imageUrl = null;
            string? publicId = null;

            if (model.ProfileImage != null && model.ProfileImage.Length > 0)
            {
                var allowedTypes = new[] { "image/jpeg", "image/png", "image/gif" };

                if (!allowedTypes.Contains(model.ProfileImage.ContentType))
                {
                    ModelState.AddModelError("ProfileImage", "Only JPG, PNG, or GIF images are allowed.");
                    model.Departments = await GetDepartmentsAsync();
                    return View(model);
                }

                var uploadResult = await _cloudinaryService.UploadImageAsync(model.ProfileImage);

                if (uploadResult == null)
                {
                    ModelState.AddModelError(string.Empty, "Image upload failed: No response from Cloudinary.");
                    model.Departments = await GetDepartmentsAsync();
                    return View(model);
                }

                if (uploadResult.Error != null)
                {
                    ModelState.AddModelError(string.Empty, $"Image upload failed: {uploadResult.Error.Message}");
                    model.Departments = await GetDepartmentsAsync();
                    return View(model);
                }

                imageUrl = uploadResult.SecureUrl?.ToString();
                publicId = uploadResult.PublicId;
            }


            var employee = new Employee
            {
                Id = Guid.NewGuid(),
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                HireDate = model.HireDate,
                Salary = model.Salary,
                DepartmentId = model.DepartmentId,
                ProfileImageUrl = imageUrl,
                ProfileImagePublicId = publicId
            };

            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Detail(Guid id)
        {
            var employee = await _context.Employees
                .Include(e => e.Department)
                .Include(e => e.Addresses)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (employee == null)
                return NotFound();

            return View(employee);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)

        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee == null)
                return NotFound();

            if (!string.IsNullOrEmpty(employee.ProfileImagePublicId))
            {
                await _cloudinaryService.DeleteImageAsync(employee.ProfileImagePublicId);
            }

            _context.Employees.Remove(employee);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }


        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee == null)
                return NotFound();

            var model = new EditEmployeeViewModel
            {
                Id = employee.Id,
                FirstName = employee.FirstName,
                LastName = employee.LastName,
                Email = employee.Email,
                HireDate = employee.HireDate,
                Salary = employee.Salary,
                DepartmentId = employee.DepartmentId,
                CurrentProfileImageUrl = employee.ProfileImageUrl,
                Departments = await GetDepartmentsAsync()
            };

            return View(model);
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditEmployeeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Departments = await GetDepartmentsAsync();
                return View(model);
            }

            var employee = await _context.Employees.FindAsync(model.Id);
            if (employee == null)
                return NotFound();

            employee.FirstName = model.FirstName;
            employee.LastName = model.LastName;
            employee.Email = model.Email;
            employee.HireDate = model.HireDate;
            employee.Salary = model.Salary;
            employee.DepartmentId = model.DepartmentId;

            if (model.ProfileImage != null && model.ProfileImage.Length > 0)
            {
                var allowedTypes = new[] { "image/jpeg", "image/png", "image/gif" };
                if (!allowedTypes.Contains(model.ProfileImage.ContentType))
                {
                    ModelState.AddModelError("ProfileImage", "Only JPG, PNG, or GIF images are allowed.");
                    model.Departments = await GetDepartmentsAsync();
                    return View(model);
                }


                if (!string.IsNullOrEmpty(employee.ProfileImagePublicId))
                {
                    await _cloudinaryService.DeleteImageAsync(employee.ProfileImagePublicId);
                }


                var uploadResult = await _cloudinaryService.UploadImageAsync(model.ProfileImage);

                if (uploadResult != null && uploadResult.Error == null)
                {
                    employee.ProfileImageUrl = uploadResult.SecureUrl?.ToString();
                    employee.ProfileImagePublicId = uploadResult.PublicId;
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Image upload failed. Please try again.");
                    model.Departments = await GetDepartmentsAsync();
                    return View(model);
                }
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }



    }
}
