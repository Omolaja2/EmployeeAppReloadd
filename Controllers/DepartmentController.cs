using Application.Dtos;
using Application.Services.Department;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Presentation.DtoMapping;
using Presentation.Models;

namespace Presentation.Controllers
{
    public class DepartmentController : BaseController
    {
        private readonly IDepartmentService _departmentService;

        public DepartmentController(IDepartmentService departmentService)
        {
            _departmentService = departmentService;
        }

        public async Task<IActionResult> Index()
        {
            var departments = await _departmentService.GetAllDepartmentsAsync();
            var viewModel = departments.ToViewModel();
            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Detail(Guid id)
        {
            var departmentDto = await _departmentService.GetDepartmentByIdAsync(id);
            if (departmentDto == null)
                return NotFound();

            var viewModel = departmentDto.ToViewModel();
            return View(viewModel);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateDepartmentViewModel model)
        {
            if (!ModelState.IsValid)
            {
                SetFlashMessage("Please fill in all required fields correctly.", "error");
                return View(model);
            }

            var viewModel = new CreateDepartmentDto
            {
                Id = Guid.NewGuid(),
                Name = model.Name,
                Description = model.Description
            };

            var result = await _departmentService.CreateDepartmentAsync(viewModel);

            if (result == null)
            {
                SetFlashMessage("An error occurred while creating the department. Please try again.", "error");
                return View(model);
            }

            SetFlashMessage("Department created successfully.", "success");
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var departmentDto = await _departmentService.GetDepartmentByIdAsync(id);
            if (departmentDto == null)
                return NotFound();

            var viewModel = new EditDepartmentViewModel
            {
                Id = departmentDto.Id,
                Name = departmentDto.Name,
                Description = departmentDto.Description
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(EditDepartmentViewModel model)
        {
            if (!ModelState.IsValid)
            {
                SetFlashMessage("Please fix the errors.", "error");
                return View(model);
            }

            var dto = new DepartmentDto
            {
                Id = model.Id,
                Name = model.Name,
                Description = model.Description
            };

            var updated = await _departmentService.UpdateDepartmentAsync(dto);
            SetFlashMessage("Department updated successfully.", "success");
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Delete(Guid id)
        {
            var departmentDto = await _departmentService.GetDepartmentByIdAsync(id);
            if (departmentDto == null)
                return NotFound();

            var viewModel = departmentDto.ToViewModel();
            return View(viewModel);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            await _departmentService.DeleteDepartmentAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
