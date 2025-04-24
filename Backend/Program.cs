using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using VizsgaremekApp.Models;
using VizsgaremekApp.Services;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

/* ===================== DATABASE CONFIGURATION ===================== */
const string internalDbUrl = "postgresql://root:hnF6KwhmAjrZONPggD3rzVYloD2tIdXa@dpg-d056ruqli9vc738uammg-a/loterdatabase";
const string externalDbUrl = "postgresql://root:hnF6KwhmAjrZONPggD3rzVYloD2tIdXa@dpg-d056ruqli9vc738uammg-a.frankfurt-postgres.render.com/loterdatabase";

try 
{
    var connectionUrl = builder.Environment.IsEnvironment("Render") ? internalDbUrl : externalDbUrl;
    var isInternal = connectionUrl == internalDbUrl;

    var databaseUri = new Uri(connectionUrl);
    var userInfo = databaseUri.UserInfo.Split(':');

    var dbConfig = new NpgsqlConnectionStringBuilder
    {
        Host = databaseUri.Host,
        Port = databaseUri.Port == -1 ? 5432 : databaseUri.Port, // Default PostgreSQL port
        Username = userInfo[0],
        Password = userInfo[1],
        Database = databaseUri.LocalPath.TrimStart('/'),
        SslMode = isInternal ? SslMode.Prefer : SslMode.Require,
        TrustServerCertificate = true,
        Pooling = true,
        CommandTimeout = 30
    };

    Console.WriteLine($"   Using {(isInternal ? "INTERNAL" : "EXTERNAL")} DB connection");
    Console.WriteLine($"   Host: {dbConfig.Host}:{dbConfig.Port}");
    Console.WriteLine($"   Database: {dbConfig.Database}");

    builder.Services.AddDbContext<VizsgaremekContext>(options => 
        options.UseNpgsql(dbConfig.ToString())
    );
}
catch (Exception ex)
{
    Console.WriteLine($" DATABASE ERROR: {ex.Message}");
    throw;
}

/* ===================== SERVICES CONFIGURATION ===================== */
builder.Services.AddControllers();
builder.Services.AddScoped<FileUploadService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "ShootingRangeAPI", Version = "v1" });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 5 * 1024 * 1024; // 5MB
});

builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddScoped<EmailService>();

/* ===================== APPLICATION SETUP ===================== */
var app = builder.Build();

// Apply migrations and ensure seed data
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<VizsgaremekContext>();
    try
    {
        Console.WriteLine(" Applying database migrations...");
        dbContext.Database.Migrate();
        Console.WriteLine(" Database ready");
    }
    catch (Exception ex)
    {
        Console.WriteLine($" MIGRATION ERROR: {ex.Message}");
        throw;
    }
}

/* ===================== MIDDLEWARE PIPELINE ===================== */
app.UseCors("AllowAll");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "ShootingRangeAPI v1"));
}

app.UseAuthorization();
app.UseStaticFiles();
app.MapControllers();

Console.WriteLine(" Application started");
app.Run();
