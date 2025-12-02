using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using BankingSystem.API.DTOs;
using BankingSystem.API.Interfaces;
using BankingSystem.API.Models;

namespace BankingSystem.API.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthService> _logger;
    private const int MaxFailedAttempts = 5;
    private const int LockoutMinutes = 30;

    public AuthService(
        IUserRepository userRepository,
        ICustomerRepository customerRepository,
        IConfiguration configuration,
        ILogger<AuthService> logger)
    {
        _userRepository = userRepository;
        _customerRepository = customerRepository;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<AuthResultDto> LoginAsync(string username, string password)
    {
        try
        {
            // Получаем пользователя
            var user = await _userRepository.GetByUsernameAsync(username);
            
            if (user == null)
            {
                return new AuthResultDto
                {
                    Success = false,
                    Message = "Неверное имя пользователя или пароль"
                };
            }

            // Проверяем, не заблокирован ли пользователь
            if (user.IsLocked)
            {
                return new AuthResultDto
                {
                    Success = false,
                    Message = $"Учетная запись заблокирована до {user.LockedUntil:dd.MM.yyyy HH:mm}"
                };
            }

            // Проверяем, активен ли пользователь
            if (!user.IsActive)
            {
                return new AuthResultDto
                {
                    Success = false,
                    Message = "Учетная запись деактивирована"
                };
            }

            // Проверяем пароль
            if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                // Увеличиваем счетчик неудачных попыток
                await _userRepository.IncrementFailedLoginAttemptsAsync(user.UserId);
                user.FailedLoginAttempts++;

                // Блокируем после максимального количества попыток
                if (user.FailedLoginAttempts >= MaxFailedAttempts)
                {
                    var lockUntil = DateTime.UtcNow.AddMinutes(LockoutMinutes);
                    await _userRepository.LockUserAsync(user.UserId, lockUntil);
                    
                    return new AuthResultDto
                    {
                        Success = false,
                        Message = $"Учетная запись заблокирована на {LockoutMinutes} минут из-за множественных неудачных попыток входа"
                    };
                }

                return new AuthResultDto
                {
                    Success = false,
                    Message = $"Неверное имя пользователя или пароль. Осталось попыток: {MaxFailedAttempts - user.FailedLoginAttempts}"
                };
            }

            // Сбрасываем счетчик неудачных попыток
            await _userRepository.ResetFailedLoginAttemptsAsync(user.UserId);
            
            // Обновляем время последнего входа
            await _userRepository.UpdateLastLoginAsync(user.UserId);

            // Генерируем JWT токен
            var token = GenerateJwtToken(user);

            // Скрываем чувствительные данные
            user.PasswordHash = string.Empty;

            return new AuthResultDto
            {
                Success = true,
                Message = "Вход выполнен успешно",
                Token = token,
                User = user
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for user: {Username}", username);
            return new AuthResultDto
            {
                Success = false,
                Message = "Ошибка при входе в систему"
            };
        }
    }

    public async Task<AuthResultDto> RegisterAsync(RegisterRequest request)
    {
        try
        {
            // Проверяем, существует ли пользователь
            if (await UserExistsAsync(request.Username))
            {
                return new AuthResultDto
                {
                    Success = false,
                    Message = "Пользователь с таким именем уже существует"
                };
            }

            // Проверяем, существует ли email
            if (await EmailExistsAsync(request.Email))
            {
                return new AuthResultDto
                {
                    Success = false,
                    Message = "Пользователь с таким email уже существует"
                };
            }

            // Хешируем пароль
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            // Создаем пользователя
            var user = new User
            {
                Username = request.Username,
                PasswordHash = passwordHash,
                Email = request.Email,
                FullName = request.FullName,
                Role = request.Role,
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            };

            user = await _userRepository.CreateAsync(user);

            // Если это клиент и есть дополнительные данные, создаем запись в Customers
            if (request.Role == "Customer" && !string.IsNullOrEmpty(request.FirstName) 
                && !string.IsNullOrEmpty(request.LastName) && request.DateOfBirth.HasValue
                && !string.IsNullOrEmpty(request.PassportNumber) && !string.IsNullOrEmpty(request.PhoneNumber))
            {
                var customer = new Customer
                {
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    MiddleName = request.MiddleName,
                    DateOfBirth = request.DateOfBirth.Value,
                    Email = request.Email,
                    PhoneNumber = request.PhoneNumber,
                    PassportNumber = request.PassportNumber,
                    Address = request.Address,
                    City = request.City,
                    Country = request.Country,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow
                };

                await _customerRepository.CreateAsync(customer);
            }

            // Генерируем токен
            var token = GenerateJwtToken(user);

            // Скрываем пароль
            user.PasswordHash = string.Empty;

            return new AuthResultDto
            {
                Success = true,
                Message = "Регистрация прошла успешно",
                Token = token,
                User = user
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration for user: {Username}", request.Username);
            return new AuthResultDto
            {
                Success = false,
                Message = "Ошибка при регистрации"
            };
        }
    }

    public async Task<bool> UserExistsAsync(string username)
    {
        var user = await _userRepository.GetByUsernameAsync(username);
        return user != null;
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        var user = await _userRepository.GetByEmailAsync(email);
        return user != null;
    }

    private string GenerateJwtToken(User user)
    {
        var jwtSettings = _configuration.GetSection("Jwt");
        var key = Encoding.UTF8.GetBytes(jwtSettings["Key"] ?? throw new InvalidOperationException("JWT Key not configured"));
        var issuer = jwtSettings["Issuer"];
        var audience = jwtSettings["Audience"];
        var expiryMinutes = int.Parse(jwtSettings["ExpiryMinutes"] ?? "60");

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim("FullName", user.FullName)
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(expiryMinutes),
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}