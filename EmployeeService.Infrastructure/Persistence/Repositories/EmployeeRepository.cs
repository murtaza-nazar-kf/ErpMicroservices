using EmployeeService.Domain.Entities;
using EmployeeService.Domain.Interfaces;

namespace EmployeeService.Infrastructure.Persistence.Repositories;

public class EmployeeRepository : IEmployeeRepository
{
    // In a real-world scenario, this would be replaced with your database context
    private readonly List<Employee> _employees = new();

    public async Task<Employee> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await Task.FromResult(_employees.FirstOrDefault(e => e.Id == id));
    }

    public async Task<IEnumerable<Employee>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await Task.FromResult(_employees);
    }

    public async Task<Employee> AddAsync(Employee employee, CancellationToken cancellationToken)
    {
        _employees.Add(employee);
        return await Task.FromResult(employee);
    }

    public async Task UpdateAsync(Employee employee, CancellationToken cancellationToken)
    {
        var existingEmployee = _employees.FirstOrDefault(e => e.Id == employee.Id);
        if (existingEmployee != null)
        {
            _employees.Remove(existingEmployee);
            _employees.Add(employee);
        }

        await Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var employee = _employees.FirstOrDefault(e => e.Id == id);
        if (employee != null) _employees.Remove(employee);
        await Task.CompletedTask;
    }
}