using Data.Context;
using Data.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Presentation.Models;
using Application.Services.UploadImage;
using Microsoft.AspNetCore.Identity;

namespace Presentation.Controllers
{
    public class EmployeeController : Controller
    {
      
        private readonly EmployeeAppDbContext _context;
        private readonly ICloudinaryService _cloudinaryService;
        private readonly  ILogger<EmployeeController> _logger;

        public EmployeeController(EmployeeAppDbContext context, ICloudinaryService cloudinaryService, ILogger<EmployeeController> logger)
        {
            _context = context;
            _cloudinaryService = cloudinaryService;
            // _signInManager = signInManager;
            _logger = logger;
            
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

            _logger.LogInformation("Acess Employee Index Page");

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
            _logger.LogInformation("Acess Employee Create Form");
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
            _logger.LogInformation("Creating New Employee : {@Model}", model);
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid Model State While Creating employee!.");
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
                    _logger.LogWarning("Invalid Image format uploaded: {ContentType",model.ProfileImage.ContentType);
                    ModelState.AddModelError("ProfileImage", "Only JPG, PNG, or GIF images are allowed.");
                    model.Departments = await GetDepartmentsAsync();
                    return View(model);
                }

                var uploadResult = await _cloudinaryService.UploadImageAsync(model.ProfileImage);

                if (uploadResult == null)
                {
                    _logger.LogWarning("Image Uploaded Fail: null response from Cloudinary. ");
                    ModelState.AddModelError(string.Empty, "Image upload failed: No response from Cloudinary.");
                    model.Departments = await GetDepartmentsAsync();
                    return View(model);
                }

                if (uploadResult.Error != null)
                {
                    _logger.LogWarning($"Error: {uploadResult.Error}");
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
            _logger.LogInformation("New employee Created Succesfully : {Email}", employee.Email);
            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        public async Task<IActionResult> Detail(Guid id)

        {
            _logger.LogInformation("Viewing details for employee {Id}", id);
            var employee = await _context.Employees
                .Include(e => e.Department)
                .Include(e => e.Addresses)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (employee == null)
                _logger.LogWarning("Employee with ID {Id} not found.", id);
            return NotFound();
            ViewBag.States = await _context.States
                .Select(s => new SelectListItem
                {
                    Value = s.Id.ToString(),
                    Text = s.Name
                }).ToListAsync();

            return View(employee);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)

        {

            _logger.LogInformation("Attempting to delete employee {Id}", id);
            var employee = await _context.Employees.FindAsync(id);

            if (employee == null)
            {
                _logger.LogWarning("Employee with ID {Id} not found during delete.", id);
                return NotFound();
            }

            if (!string.IsNullOrEmpty(employee.ProfileImagePublicId))
            {
                await _cloudinaryService.DeleteImageAsync(employee.ProfileImagePublicId);
            }

            _context.Employees.Remove(employee);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Employee {Email} deleted successfully.", employee.Email);
            return RedirectToAction(nameof(Index));
        }


        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            _logger.LogInformation("Accessed edit form for employee {Id}", id);
            var employee = await _context.Employees.FindAsync(id);
            if (employee == null)
               {
                _logger.LogWarning("Employee with ID {Id} not found during edit GET.", id);
                return NotFound();
            }

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
            _logger.LogInformation("Updating employee {Id}", model.Id);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state while editing employee {Id}", model.Id);
                model.Departments = await GetDepartmentsAsync();
                return View(model);
            }

            var employee = await _context.Employees.FindAsync(model.Id);
            if (employee == null)
            {
                _logger.LogWarning("Employee with ID {Id} not found during edit POST.", model.Id);
                return NotFound();
            }

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
                    _logger.LogWarning("Invalid image format uploaded during edit: {ContentType}", model.ProfileImage.ContentType);
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
                    _logger.LogError("Image upload failed for employee {Id}.", model.Id);
                    ModelState.AddModelError(string.Empty, "Image upload failed. Please try again.");
                    model.Departments = await GetDepartmentsAsync();
                    return View(model);
                }
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Employee updated successfully!";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProfileImage(Guid id)
        {
            _logger.LogInformation("Deleting profile image for employee {Id}", id);
            var employee = await _context.Employees.FindAsync(id);
            if (employee == null)
            {
                _logger.LogWarning("Employee with ID {Id} not found during profile image deletion.", id);
                return NotFound();
            }

            if (!string.IsNullOrEmpty(employee.ProfileImagePublicId))
            {
                await _cloudinaryService.DeleteImageAsync(employee.ProfileImagePublicId);
            }

            employee.ProfileImageUrl = null;
            employee.ProfileImagePublicId = null;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Employee {Id} updated successfully.", id);
            TempData["Success"] = "Profile image deleted successfully.";

            return RedirectToAction(nameof(Detail), new { id = employee.Id });
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddAddress(Guid EmployeeId, string Street, string City, Guid StateId, string ZipCode)
        {
            _logger.LogInformation("Adding address for employee {EmployeeId}", EmployeeId);
            var employee = await _context.Employees.FindAsync(EmployeeId);
            if (employee == null)
            {
                _logger.LogWarning("Employee with ID {EmployeeId} not found while adding address.", EmployeeId);
                return NotFound();
            }

            var address = new Adress
            {
                EmployeeId = EmployeeId,
                Street = Street,
                City = City,
                StateId = StateId,
                ZipCode = ZipCode
            };

            _context.Addresses.Add(address);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Address added successfully.";
            return RedirectToAction(nameof(Detail), new { id = EmployeeId });
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAddress(Guid addressId, Guid employeeId)
        {
            _logger.LogInformation("Deleting address {AddressId} for employee {EmployeeId}", addressId, employeeId);

            var address = await _context.Addresses.FindAsync(addressId);

            if (address == null)
            {
                _logger.LogWarning("Address with ID {AddressId} not found during delete.", addressId);
                return NotFound();
            }

            _context.Addresses.Remove(address);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Address deleted successfully.";
            return RedirectToAction(nameof(Detail), new { id = employeeId });

        }
       


    }
}
