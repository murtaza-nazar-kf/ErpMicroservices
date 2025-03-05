using Microsoft.EntityFrameworkCore;
using UserService.Domain.Entities;
using UserService.Domain.Interfaces;
using UserService.Infrastructure.Persistence;

namespace UserService.Infrastructure.Repositories;

public class UserRepository(UserDbContext context) : IUserRepository
{
    public Task<User?> GetByIdAsync(Guid id)
    {
        return context.Users
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public Task<User?> GetByEmailAsync(string email)
    {
        return context.Users
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    public Task<User?> GetByUsernameAsync(string username)
    {
        return context.Users
            .FirstOrDefaultAsync(u => u.Username == username);
    }

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        return await context.Users.ToListAsync().ConfigureAwait(true);
    }

    public async Task<User> CreateAsync(User user)
    {
        await context.Users.AddAsync(user).ConfigureAwait(true);
        await context.SaveChangesAsync().ConfigureAwait(true);
        return user;
    }

    public Task UpdateAsync(User user)
    {
        context.Entry(user).State = EntityState.Modified;
        return context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var user = await GetByIdAsync(id).ConfigureAwait(true);
        if (user != null)
        {
            context.Users.Remove(user);
            await context.SaveChangesAsync().ConfigureAwait(true);
        }
    }

    public Task<bool> ExistsAsync(Guid id)
    {
        return context.Users
            .AnyAsync(u => u.Id == id);
    }

    public Task<bool> EmailExistsAsync(string email)
    {
        return context.Users
            .AnyAsync(u => u.Email == email);
    }

    public Task<bool> UsernameExistsAsync(string username)
    {
        return context.Users
            .AnyAsync(u => u.Username == username);
    }

    public async Task<(bool IsValid, string UserId)> ValidateUserAsync(string email, string password)
    {
        var user = await context.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null) return (false, string.Empty);

        var isValid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
        return (isValid, isValid ? user.Id.ToString() : string.Empty);
    }
}