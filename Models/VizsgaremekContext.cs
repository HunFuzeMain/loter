using Microsoft.EntityFrameworkCore;
using tempbackend.Models;

namespace VizsgaremekApp.Models
{
    public class VizsgaremekContext : DbContext
    {
        public VizsgaremekContext(DbContextOptions<VizsgaremekContext> options)
            : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySql(
                "server=localhost;database=vizsgaremek;user=root;password=;",
                ServerVersion.AutoDetect("server=localhost;database=vizsgaremek;user=root;password=;")
            );
        }


        public DbSet<Registration> Registrations { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<Instructor> Instructors { get; set; }
        public DbSet<Package> Packages { get; set; }
        public DbSet<Question> Questions { get; set; } // DbSet for TextData


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Instructor>().HasData(
                new Instructor { Id = 1, Name = "Nagy Gergely" },
                new Instructor { Id = 2, Name = "Kovács Ákos" },
                new Instructor { Id = 3, Name = "Tőth László" },
                new Instructor { Id = 4, Name = "Szabó Tamás" },
                new Instructor { Id = 5, Name = "Horváth Márton" }
            );

            modelBuilder.Entity<Package>().HasData(
                new Package { Id = 1, Name = "Különleges Alakulat", Price = 45000 },
                new Package { Id = 2, Name = "Orosz", Price = 25500 },
                new Package { Id = 3, Name = "Magyar", Price = 30000 },
                new Package { Id = 4, Name = "9mm-es pisztoly", Price = 20000 },
                new Package { Id = 5, Name = "9mm-es géppisztoly", Price = 40000 },
                new Package { Id = 6, Name = "5.56 NATO és .300 blackout", Price = 75000 },
                new Package { Id = 7, Name = "7.62-es", Price = 45000 }
            );
        }
    }
}