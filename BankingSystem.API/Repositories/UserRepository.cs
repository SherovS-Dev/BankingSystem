using Npgsql;
using BankingSystem.API.Interfaces;
using BankingSystem.API.Models;

namespace BankingSystem.API.Repositories;

public class UserRepository : IUserRepository
{
    private readonly string _connectionString;

    public UserRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(@"
            SELECT userid, username, passwordhash, email, fullname, role, 
                   isactive, createddate, lastlogindate, failedloginattempts, lockeduntil
            FROM users 
            WHERE userid = @Id", connection);
        
        command.Parameters.AddWithValue("@Id", id);

        await using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return MapUser(reader);
        }

        return null;
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(@"
            SELECT userid, username, passwordhash, email, fullname, role, 
                   isactive, createddate, lastlogindate, failedloginattempts, lockeduntil
            FROM users 
            WHERE LOWER(username) = LOWER(@Username)", connection);
        
        command.Parameters.AddWithValue("@Username", username);

        await using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return MapUser(reader);
        }

        return null;
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(@"
            SELECT userid, username, passwordhash, email, fullname, role, 
                   isactive, createddate, lastlogindate, failedloginattempts, lockeduntil
            FROM users 
            WHERE LOWER(email) = LOWER(@Email)", connection);
        
        command.Parameters.AddWithValue("@Email", email);

        await using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return MapUser(reader);
        }

        return null;
    }

    public async Task<User> CreateAsync(User user)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(@"
            INSERT INTO users (username, passwordhash, email, fullname, role, isactive, createddate)
            VALUES (@Username, @PasswordHash, @Email, @FullName, @Role, @IsActive, @CreatedDate)
            RETURNING userid", connection);

        command.Parameters.AddWithValue("@Username", user.Username);
        command.Parameters.AddWithValue("@PasswordHash", user.PasswordHash);
        command.Parameters.AddWithValue("@Email", user.Email);
        command.Parameters.AddWithValue("@FullName", user.FullName);
        command.Parameters.AddWithValue("@Role", user.Role);
        command.Parameters.AddWithValue("@IsActive", user.IsActive);
        command.Parameters.AddWithValue("@CreatedDate", DateTime.UtcNow);

        user.UserId = (int)(await command.ExecuteScalarAsync() ?? 0);
        return user;
    }

    public async Task UpdateAsync(User user)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(@"
            UPDATE users 
            SET email = @Email,
                fullname = @FullName,
                role = @Role,
                isactive = @IsActive
            WHERE userid = @UserId", connection);

        command.Parameters.AddWithValue("@UserId", user.UserId);
        command.Parameters.AddWithValue("@Email", user.Email);
        command.Parameters.AddWithValue("@FullName", user.FullName);
        command.Parameters.AddWithValue("@Role", user.Role);
        command.Parameters.AddWithValue("@IsActive", user.IsActive);

        await command.ExecuteNonQueryAsync();
    }

    public async Task UpdateLastLoginAsync(int userId)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(@"
            UPDATE users 
            SET lastlogindate = @LastLoginDate
            WHERE userid = @UserId", connection);

        command.Parameters.AddWithValue("@UserId", userId);
        command.Parameters.AddWithValue("@LastLoginDate", DateTime.UtcNow);

        await command.ExecuteNonQueryAsync();
    }

    public async Task IncrementFailedLoginAttemptsAsync(int userId)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(@"
            UPDATE users 
            SET failedloginattempts = failedloginattempts + 1
            WHERE userid = @UserId", connection);

        command.Parameters.AddWithValue("@UserId", userId);

        await command.ExecuteNonQueryAsync();
    }

    public async Task ResetFailedLoginAttemptsAsync(int userId)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(@"
            UPDATE users 
            SET failedloginattempts = 0,
                lockeduntil = NULL
            WHERE userid = @UserId", connection);

        command.Parameters.AddWithValue("@UserId", userId);

        await command.ExecuteNonQueryAsync();
    }

    public async Task LockUserAsync(int userId, DateTime lockUntil)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(@"
            UPDATE users 
            SET lockeduntil = @LockUntil
            WHERE userid = @UserId", connection);

        command.Parameters.AddWithValue("@UserId", userId);
        command.Parameters.AddWithValue("@LockUntil", lockUntil);

        await command.ExecuteNonQueryAsync();
    }

    private static User MapUser(NpgsqlDataReader reader)
    {
        return new User
        {
            UserId = reader.GetInt32(0),
            Username = reader.GetString(1),
            PasswordHash = reader.GetString(2),
            Email = reader.GetString(3),
            FullName = reader.GetString(4),
            Role = reader.GetString(5),
            IsActive = reader.GetBoolean(6),
            CreatedDate = reader.GetDateTime(7),
            LastLoginDate = reader.IsDBNull(8) ? null : reader.GetDateTime(8),
            FailedLoginAttempts = reader.GetInt32(9),
            LockedUntil = reader.IsDBNull(10) ? null : reader.GetDateTime(10)
        };
    }
}