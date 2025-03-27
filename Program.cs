using Microsoft.EntityFrameworkCore;
using VizsgaremekApp.Models;

var builder = WebApplication.CreateBuilder(args);

// Add DbContext and configure the connection string
builder.Services.AddDbContext<VizsgaremekContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))
    )
);

// Add services to the container.
builder.Services.AddControllers();

// Add Swagger for API documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "ShootingRangeAPI", Version = "v1" });
});

// Add CORS
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

// Use CORS
app.UseCors("AllowAll");


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "ShootingRangeAPI v1"));
}

app.UseAuthorization();
app.MapControllers();

app.Run();

// Seed data for the database
public static class SeedData
{
    public static void Initialize(IServiceProvider serviceProvider)
    {
        using (var context = new VizsgaremekContext(
            serviceProvider.GetRequiredService<DbContextOptions<VizsgaremekContext>>()))
        {
            // Check if the database already has data
            if (context.Instructors.Any() || context.Packages.Any())
            {
                return; // Database has been seeded
            }

            // Seed Instructors
            context.Instructors.AddRange(
                new Instructor { Id = 1, Name = "Nagy Gergely" },
                new Instructor { Id = 2, Name = "Kovács Ákos" },
                new Instructor { Id = 3, Name = "Tõth László" },
                new Instructor { Id = 4, Name = "Szabó Tamás" },
                new Instructor { Id = 5, Name = "Horváth Márton" }
            );

            // Seed Packages
            context.Packages.AddRange(
                new Package { Id = 1, Name = "Különleges Alakulat", Price = 45000 },
                new Package { Id = 2, Name = "Orosz", Price = 25500 },
                new Package { Id = 3, Name = "Magyar", Price = 30000 },
                new Package { Id = 4, Name = "9mm-es pisztoly", Price = 20000 },
                new Package { Id = 5, Name = "9mm-es géppisztoly", Price = 40000 },
                new Package { Id = 6, Name = "5.56 NATO és .300 blackout", Price = 75000 },
                new Package { Id = 7, Name = "7.62-es", Price = 45000 }
            );

            context.SaveChanges();
        }
    }
}