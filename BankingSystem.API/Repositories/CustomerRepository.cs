using Npgsql;
using BankingSystem.API.Interfaces;
using BankingSystem.API.Models;

namespace BankingSystem.API.Repositories;

public class CustomerRepository : ICustomerRepository
{
    private readonly string _connectionString;

    public CustomerRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<Customer?> GetByIdAsync(int id)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(@"
            SELECT customerid, firstname, lastname, middlename, dateofbirth, email, 
                   phonenumber, passportnumber, address, city, country, 
                   createddate, updateddate, isactive
            FROM customers 
            WHERE customerid = @Id", connection);
        
        command.Parameters.AddWithValue("@Id", id);

        await using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return MapCustomer(reader);
        }

        return null;
    }

    public async Task<Customer?> GetByEmailAsync(string email)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(@"
            SELECT customerid, firstname, lastname, middlename, dateofbirth, email, 
                   phonenumber, passportnumber, address, city, country, 
                   createddate, updateddate, isactive
            FROM customers 
            WHERE LOWER(email) = LOWER(@Email)", connection);
        
        command.Parameters.AddWithValue("@Email", email);

        await using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return MapCustomer(reader);
        }

        return null;
    }

    public async Task<Customer?> GetByPassportNumberAsync(string passportNumber)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(@"
            SELECT customerid, firstname, lastname, middlename, dateofbirth, email, 
                   phonenumber, passportnumber, address, city, country, 
                   createddate, updateddate, isactive
            FROM customers 
            WHERE passportnumber = @PassportNumber", connection);
        
        command.Parameters.AddWithValue("@PassportNumber", passportNumber);

        await using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return MapCustomer(reader);
        }

        return null;
    }

    public async Task<IEnumerable<Customer>> GetAllAsync()
    {
        var customers = new List<Customer>();

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(@"
            SELECT customerid, firstname, lastname, middlename, dateofbirth, email, 
                   phonenumber, passportnumber, address, city, country, 
                   createddate, updateddate, isactive
            FROM customers 
            WHERE isactive = TRUE
            ORDER BY createddate DESC", connection);

        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            customers.Add(MapCustomer(reader));
        }

        return customers;
    }

    public async Task<Customer> CreateAsync(Customer customer)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(@"
            INSERT INTO customers (firstname, lastname, middlename, dateofbirth, email, 
                                  phonenumber, passportnumber, address, city, country, 
                                  createddate, updateddate, isactive)
            VALUES (@FirstName, @LastName, @MiddleName, @DateOfBirth, @Email, 
                   @PhoneNumber, @PassportNumber, @Address, @City, @Country,
                   @CreatedDate, @UpdatedDate, @IsActive)
            RETURNING customerid", connection);

        command.Parameters.AddWithValue("@FirstName", customer.FirstName);
        command.Parameters.AddWithValue("@LastName", customer.LastName);
        command.Parameters.AddWithValue("@MiddleName", customer.MiddleName ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@DateOfBirth", customer.DateOfBirth);
        command.Parameters.AddWithValue("@Email", customer.Email);
        command.Parameters.AddWithValue("@PhoneNumber", customer.PhoneNumber);
        command.Parameters.AddWithValue("@PassportNumber", customer.PassportNumber);
        command.Parameters.AddWithValue("@Address", customer.Address ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@City", customer.City ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@Country", customer.Country);
        command.Parameters.AddWithValue("@CreatedDate", DateTime.UtcNow);
        command.Parameters.AddWithValue("@UpdatedDate", DateTime.UtcNow);
        command.Parameters.AddWithValue("@IsActive", customer.IsActive);

        customer.CustomerId = (int)(await command.ExecuteScalarAsync() ?? 0);
        return customer;
    }

    public async Task UpdateAsync(Customer customer)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(@"
            UPDATE customers 
            SET firstname = @FirstName,
                lastname = @LastName,
                middlename = @MiddleName,
                email = @Email,
                phonenumber = @PhoneNumber,
                address = @Address,
                city = @City,
                country = @Country,
                updateddate = @UpdatedDate,
                isactive = @IsActive
            WHERE customerid = @CustomerId", connection);

        command.Parameters.AddWithValue("@CustomerId", customer.CustomerId);
        command.Parameters.AddWithValue("@FirstName", customer.FirstName);
        command.Parameters.AddWithValue("@LastName", customer.LastName);
        command.Parameters.AddWithValue("@MiddleName", customer.MiddleName ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@Email", customer.Email);
        command.Parameters.AddWithValue("@PhoneNumber", customer.PhoneNumber);
        command.Parameters.AddWithValue("@Address", customer.Address ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@City", customer.City ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@Country", customer.Country);
        command.Parameters.AddWithValue("@UpdatedDate", DateTime.UtcNow);
        command.Parameters.AddWithValue("@IsActive", customer.IsActive);

        await command.ExecuteNonQueryAsync();
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(@"
            SELECT COUNT(1) 
            FROM customers 
            WHERE LOWER(email) = LOWER(@Email)", connection);
        
        command.Parameters.AddWithValue("@Email", email);

        var result = await command.ExecuteScalarAsync();
        return result != null && Convert.ToInt32(result) > 0;
    }

    public async Task<bool> PassportExistsAsync(string passportNumber)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(@"
            SELECT COUNT(1) 
            FROM customers 
            WHERE passportnumber = @PassportNumber", connection);
        
        command.Parameters.AddWithValue("@PassportNumber", passportNumber);

        var result = await command.ExecuteScalarAsync();
        return result != null && Convert.ToInt32(result) > 0;
    }

    private static Customer MapCustomer(NpgsqlDataReader reader)
    {
        return new Customer
        {
            CustomerId = reader.GetInt32(0),
            FirstName = reader.GetString(1),
            LastName = reader.GetString(2),
            MiddleName = reader.IsDBNull(3) ? null : reader.GetString(3),
            DateOfBirth = reader.GetDateTime(4),
            Email = reader.GetString(5),
            PhoneNumber = reader.GetString(6),
            PassportNumber = reader.GetString(7),
            Address = reader.IsDBNull(8) ? null : reader.GetString(8),
            City = reader.IsDBNull(9) ? null : reader.GetString(9),
            Country = reader.GetString(10),
            CreatedDate = reader.GetDateTime(11),
            UpdatedDate = reader.GetDateTime(12),
            IsActive = reader.GetBoolean(13)
        };
    }
}