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
        Description = "API для управления банковскими операциями",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "Banking System",
            Email = "support@bank.tj"
        }
    });
});

// Регистрируем репозитории
builder.Services.AddScoped<IAccountRepository>(sp => new AccountRepository(connectionString));
builder.Services.AddScoped<ITransactionRepository>(sp => new TransactionRepository(connectionString));

// Регистрируем сервисы
builder.Services.AddScoped<ITransactionService>(sp =>
    new TransactionService(
        sp.GetRequiredService<ITransactionRepository>(),
        connectionString,
        sp.GetRequiredService<ILogger<TransactionService>>()
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
app.UseAuthorization();
app.MapControllers();

Console.WriteLine("═══════════════════════════════════════════════════════");
Console.WriteLine("🚀 Banking System API успешно запущен!");
Console.WriteLine("═══════════════════════════════════════════════════════");
Console.WriteLine($"📊 Swagger UI: {(app.Environment.IsDevelopment() ? "http://localhost:5000" : "")}");
Console.WriteLine($"🌐 API Base URL: http://localhost:5000/api");
Console.WriteLine("═══════════════════════════════════════════════════════");

app.Run();