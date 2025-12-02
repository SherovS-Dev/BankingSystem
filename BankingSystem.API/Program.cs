using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using BankingSystem.API.Interfaces;
using BankingSystem.API.Repositories;
using BankingSystem.API.Services;

var builder = WebApplication.CreateBuilder(args);

// Получаем connection string
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// Добавляем сервисы
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Banking System API",
        Version = "v1",
        Description = "API для управления банковскими операциями с авторизацией",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "Banking System",
            Email = "support@bank.tj"
        }
    });

    // Добавляем поддержку JWT в Swagger
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Настройка JWT Authentication
var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwtSettings["Key"] ?? throw new InvalidOperationException("JWT Key not configured"));

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; // В production установить true
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// Регистрируем репозитории
builder.Services.AddScoped<IAccountRepository>(_ => new AccountRepository(connectionString));
builder.Services.AddScoped<ITransactionRepository>(_ => new TransactionRepository(connectionString));
builder.Services.AddScoped<IUserRepository>(_ => new UserRepository(connectionString));
builder.Services.AddScoped<ICustomerRepository>(_ => new CustomerRepository(connectionString));

// Регистрируем сервисы
builder.Services.AddScoped<ITransactionService>(sp =>
    new TransactionService(
        sp.GetRequiredService<ITransactionRepository>(),
        connectionString,
        sp.GetRequiredService<ILogger<TransactionService>>()
    ));

builder.Services.AddScoped<IAuthService>(sp =>
    new AuthService(
        sp.GetRequiredService<IUserRepository>(),
        sp.GetRequiredService<ICustomerRepository>(),
        sp.GetRequiredService<IConfiguration>(),
        sp.GetRequiredService<ILogger<AuthService>>()
    ));

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Banking System API V1");
        c.RoutePrefix = string.Empty; // Swagger UI на корневом пути
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");

// ВАЖНО: Authentication должна быть перед Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

Console.WriteLine("═══════════════════════════════════════════════════════");
Console.WriteLine("🚀 Banking System API с авторизацией успешно запущен!");
Console.WriteLine("═══════════════════════════════════════════════════════");
Console.WriteLine($"📊 Swagger UI: http://localhost:5186");
Console.WriteLine($"🌐 API Base URL: http://localhost:5186/api");
Console.WriteLine($"🔐 JWT Authentication: Enabled");
Console.WriteLine("═══════════════════════════════════════════════════════");
Console.WriteLine("Доступные endpoints:");
Console.WriteLine("  POST /api/auth/login - Вход в систему");
Console.WriteLine("  POST /api/auth/register - Регистрация");
Console.WriteLine("  GET  /api/auth/verify - Проверка токена");
Console.WriteLine("═══════════════════════════════════════════════════════");

app.Run();