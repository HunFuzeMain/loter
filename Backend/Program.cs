using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using VizsgaremekApp.Models;
using VizsgaremekApp.Services;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

/* ====================================================
   DATABASE CONFIGURATION (Render.com PostgreSQL)
   ==================================================== */
const string internalDbUrl = "postgresql://root:hnF6KwhmAjrZONPggD3rzVYloD2tIdXa@dpg-d056ruqli9vc738uammg-a/loterdatabase";
const string externalDbUrl = "postgresql://root:hnF6KwhmAjrZONPggD3rzVYloD2tIdXa@dpg-d056ruqli9vc738uammg-a.frankfurt-postgres.render.com/loterdatabase";

try
{
    // Auto-select connection based on environment
    var connectionUrl = builder.Environment.IsEnvironment("Render") ? internalDbUrl : externalDbUrl;
    var isInternal = connectionUrl == internalDbUrl;

    var databaseUri = new Uri(connectionUrl);
    var userInfo = databaseUri.UserInfo.Split(':');

    var dbConfig = new NpgsqlConnectionStringBuilder
    {
        Host = databaseUri.Host,
        Port = databaseUri.Port,
        Username = userInfo[0],
        Password = userInfo[1],
        Database = databaseUri.LocalPath.TrimStart('/'),
        SslMode = isInternal ? SslMode.Prefer : SslMode.Require,
        TrustServerCertificate = true,
        Pooling = true,
        MinPoolSize = 1,
        MaxPoolSize = 20,
        ConnectionIdleLifetime = 300,
        CommandTimeout = 30
    };

    Console.WriteLine($"   Using {(isInternal ? "INTERNAL" : "EXTERNAL")} PostgreSQL connection");
    Console.WriteLine($"   Host: {databaseUri.Host}");
    Console.WriteLine($"   Database: {dbConfig.Database}");

    builder.Services.AddDbContext<VizsgaremekContext>(options =>
        options.UseNpgsql(dbConfig.ToString())
    );
}
catch (Exception ex)
{
    Console.WriteLine($" CRITICAL DATABASE ERROR: {ex.Message}");
    throw;
}

/* ====================================================
   SERVICES CONFIGURATION
   ==================================================== */
builder.Services.AddControllers();
builder.Services.AddScoped<FileUploadService>();

// Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "ShootingRangeAPI", Version = "v1" });
});

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

// File Upload
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 5 * 1024 * 1024; // 5MB
});

// Email Service
builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddScoped<EmailService>();

/* ====================================================
   APPLICATION BUILD
   ==================================================== */
var app = builder.Build();

/* ====================================================
   DATABASE MIGRATION
   ==================================================== */
try
{
    Console.WriteLine(" Applying database migrations...");
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<VizsgaremekContext>();
    dbContext.Database.Migrate();
    Console.WriteLine(" Database migrations complete");
}
catch (Exception ex)
{
    Console.WriteLine($" DATABASE MIGRATION FAILED: {ex.Message}");
    throw;
}

/* ====================================================
   MIDDLEWARE PIPELINE
   ==================================================== */
app.UseCors("AllowAll");

// Swagger in development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "ShootingRangeAPI v1");
        c.ConfigObject.DisplayRequestDuration = true;
    });
}

app.UseAuthorization();
app.UseStaticFiles();
app.MapControllers();

Console.WriteLine(" Application startup complete");
app.Run();
