using EmployeeService.Domain.Entities;
using EmployeeService.Domain.Interfaces;
using EmployeeService.Infrastructure.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace EmployeeService.Infrastructure.Persistence.Repositories;

public class EmployeeRepository(EmployeeDbContext dbContext) : IEmployeeRepository
{
    public Task<Employee?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return dbContext.Employees
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Employee>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await dbContext.Employees
            .ToListAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<Employee> AddAsync(Employee employee, CancellationToken cancellationToken)
    {
        await dbContext.Employees.AddAsync(employee, cancellationToken).ConfigureAwait(false);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return employee;
    }

    public Task UpdateAsync(Employee employee, CancellationToken cancellationToken)
    {
        dbContext.Employees.Update(employee);
        return dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var employee = await dbContext.Employees
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken).ConfigureAwait(false);

        if (employee != null)
        {
            dbContext.Employees.Remove(employee);
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}