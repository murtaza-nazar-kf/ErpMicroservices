using EmployeeService.Domain.Entities;

namespace EmployeeService.Domain.Interfaces;

public interface IEmployeeRepository
{
    Task<Employee?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IEnumerable<Employee>> GetAllAsync(CancellationToken cancellationToken);
    Task<Employee> AddAsync(Employee employee, CancellationToken cancellationToken);
    Task UpdateAsync(Employee employee, CancellationToken cancellationToken);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
}