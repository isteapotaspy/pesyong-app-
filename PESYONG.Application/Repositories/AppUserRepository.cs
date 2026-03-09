using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PESYONG.Domain.Entities.Users.Identity;
using PESYONG.Infrastructure;

namespace PESYONG.ApplicationLogic.Repositories;

public class AppUserRepository
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;

    public AppUserRepository(IDbContextFactory<AppDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    /// <summary>
    /// Creates a user in the database.
    /// </summary>
    public async Task CreateUserAsync(AppUser user)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        context.AppUsers.Add(user);
        Debug.WriteLine($"\n\nThe user has ID of {user.Id} and is named {user.UserName}\n\n");
        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Creates a user in the database and returns the created user with its assigned UserID.
    /// </summary>
    public async Task<AppUser> CreateUserAsyncReturnSelf(AppUser user)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        context.AppUsers.Add(user);
        await context.SaveChangesAsync();

        Debug.WriteLine($"\n\nThe user has ID of {user.Id} and is named {user.UserName}\n\n");
        return user;
    }

    /// <summary>
    /// Grabs a user by its ID. Returns null if not found.
    /// </summary>
    public async Task<AppUser?> GetUserByIdAsync(int id)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            return await context.AppUsers
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == id);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"GetUserByIdAsync error: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Grabs a user by their username. Returns null if not found.
    /// </summary>
    public async Task<AppUser?> GetUserByUsernameAsync(string username)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            return await context.AppUsers
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.UserName == username);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"GetUserByUsernameAsync error: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Grabs a user by their email. Returns null if not found.
    /// </summary>
    public async Task<AppUser?> GetUserByEmailAsync(string email)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            return await context.AppUsers
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email == email);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"GetUserByEmailAsync error: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Returns all the users in the table.
    /// </summary>
    public async Task<List<AppUser>> GetAllUsersAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var users = await context.AppUsers
            .AsNoTracking()
            .ToListAsync();

        if (users == null || !users.Any())
        {
            Debug.WriteLine("No users found in database");
            return new List<AppUser>();
        }

        Debug.WriteLine($"Retrieved {users.Count} users from database");
        return users;
    }

    /// <summary>
    /// Allows for complex query to occur.
    /// </summary>
    public async Task<List<AppUser>> GetUsersAsync(Func<IQueryable<AppUser>, IQueryable<AppUser>> queryBuilder)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        IQueryable<AppUser> query = context.AppUsers.AsQueryable();
        query = queryBuilder(query);

        return await query.AsNoTracking().ToListAsync();
    }

    /// <summary>
    /// Updates a user. Pass through the entire user object and update by its delta.
    /// </summary>
    public async Task UpdateUserAsync(AppUser user)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        context.AppUsers.Update(user);
        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Deletes a user in the database.
    /// </summary>
    public async Task DeleteUserAsync(int userId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var user = await context.AppUsers.FindAsync(userId);
        if (user != null)
        {
            context.AppUsers.Remove(user);
            await context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Checks if a username already exists in the database.
    /// </summary>
    public async Task<bool> UsernameExistsAsync(string username)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        return await context.AppUsers
            .AsNoTracking()
            .AnyAsync(u => u.UserName == username);
    }

    /// <summary>
    /// Checks if an email already exists in the database.
    /// </summary>
    public async Task<bool> EmailExistsAsync(string email)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        return await context.AppUsers
            .AsNoTracking()
            .AnyAsync(u => u.Email == email);
    }
}