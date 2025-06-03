using Application.Dtos;
using Presentation.Models;

namespace Presentation.DtoMapping;

public static class Mapperly
{
   
    public static DepartmentViewModel ToViewModel(this DepartmentDto dto)
    {
        return new DepartmentViewModel()
        {
            Id = dto.Id,
            Name = dto.Name,
            Description = dto.Description
        };
    }

    public static DepartmentDto ToDto(this DepartmentViewModel vm)
    {
        return new DepartmentDto()
        {
            Id = vm.Id,
            Name = vm.Name,
            Description = vm.Description,
            Employees = []
        };
    }

    
    public static DepartmentViewModel ToViewModel(this DepartmentsDto dto)
    {
        if (dto == null || dto.Departments == null)
        {
            return new DepartmentViewModel()
            {
                Departments = new List<DepartmentViewModel>()  
            };
        }

        return new DepartmentViewModel()
        {
            Departments = dto.Departments.Select(d => d.ToViewModel()).ToList()
        };
    }

}
