using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PESYONG.Domain.Entities.Users;
using PESYONG.Infrastructure;


namespace PESYONG.ApplicationLogic.Repositories;

public class CustomerRepository
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;

    public CustomerRepository(IDbContextFactory<AppDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    /// <summary>
    /// Creates a customer in the database.
    /// </summary>
    /// <param name="customer"></param>
    /// <returns></returns>
    public async Task CreateCustomerAsync(Customer customer)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        context.Customers.Add(customer);
        Debug.WriteLine($"\n\nThe customer has ID of {customer.CustomerID} and is named {customer.FirstName} {customer.LastName}\n\n");
        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Creates a customer in the database and returns the created customer with its assigned CustomerID.
    /// </summary>
    /// <param name="customer"></param>
    /// <returns>customer</returns>
    public async Task<Customer> CreateCustomerAsyncReturnSelf(Customer customer)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        context.Customers.Add(customer);
        await context.SaveChangesAsync();

        Debug.WriteLine($"\n\nThe customer has ID of {customer.CustomerID} and is named {customer.FirstName} {customer.LastName}\n\n");
        return customer;
    }

    /// <summary>
    /// Grabs a customer by its ID. Returns null if not found.
    /// </summary>
    /// <param name="id"></param>
    /// <returns>customer</returns>
    public async Task<Customer?> GetCustomerByIdAsync(Guid id)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            return await context.Customers
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.CustomerID == id);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"GetCustomerByIdAsync error: {ex.Message}");
            return null;
        }
    }

    // <summary>
    /// Returns all the customers in the table.
    /// </summary>
    /// <returns>List<Customer></returns>
    public async Task<List<Customer>> GetAllCustomersAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var customers = await context.Customers
            .AsNoTracking()
            .ToListAsync();

        if (customers == null || !customers.Any())
        {
            Debug.WriteLine("No customers found in database");
            return new List<Customer>();
        }

        Debug.WriteLine($"Retrieved {customers.Count} customers from database");
        return customers;
    }

    /// <summary>
    /// Gets customers by email address. Returns null if not found.
    /// </summary>
    /// <param name="email"></param>
    /// <returns>Customer?</returns>
    public async Task<Customer?> GetCustomerByEmailAsync(string email)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            return await context.Customers
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Email == email);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"GetCustomerByEmailAsync error: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Gets active customers only.
    /// </summary>
    /// <returns>List<Customer></returns>
    public async Task<List<Customer>> GetActiveCustomersAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var customers = await context.Customers
            .AsNoTracking()
            .Where(c => c.IsActive)
            .ToListAsync();

        Debug.WriteLine($"Retrieved {customers.Count} active customers from database");
        return customers;
    }

    /// <summary>
    /// Allows for complex query to occur.
    /// </summary>
    /// <param name="queryBuilder"></param>
    /// <returns>List<Customer></returns>
    public async Task<List<Customer>> GetCustomersAsync(Func<IQueryable<Customer>, IQueryable<Customer>> queryBuilder)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        IQueryable<Customer> query = context.Customers.AsQueryable();
        query = queryBuilder(query);

        return await query.AsNoTracking().ToListAsync();
    }

    //// <summary>
    /// Updates a customer. Pass through the entire customer object and update by its delta.
    /// </summary>
    /// <param name="customer"></param>
    /// <returns></returns>
    public async Task UpdateCustomerAsync(Customer customer)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        context.Customers.Update(customer);
        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Soft deletes a customer by setting IsActive to false.
    /// </summary>
    /// <param name="customerId"></param>
    /// <returns></returns>
    public async Task SoftDeleteCustomerAsync(Guid customerId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var customer = await context.Customers.FindAsync(customerId);
        if (customer != null)
        {
            customer.IsActive = false;
            context.Customers.Update(customer);
            await context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Hard deletes a customer from the database.
    /// </summary>
    /// <param name="customerId"></param>
    /// <returns></returns>
    public async Task DeleteCustomerAsync(Guid customerId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var customer = await context.Customers.FindAsync(customerId);
        if (customer != null)
        {
            context.Customers.Remove(customer);
            await context.SaveChangesAsync();
        }
    }
}

