using System.ComponentModel.DataAnnotations;

namespace BankingSystem.API.DTOs;

public class RegisterRequest
{
    [Required(ErrorMessage = "Имя пользователя обязательно")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Имя пользователя должно быть от 3 до 50 символов")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Пароль обязателен")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Пароль должен быть минимум 6 символов")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email обязателен")]
    [EmailAddress(ErrorMessage = "Некорректный email адрес")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Полное имя обязательно")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Полное имя должно быть от 3 до 100 символов")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Роль обязательна")]
    public string Role { get; set; } = "Customer"; // По умолчанию Customer

    // Связанные данные клиента (опционально при регистрации)
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? MiddleName { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? PhoneNumber { get; set; }
    public string? PassportNumber { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string Country { get; set; } = "Tajikistan";
}