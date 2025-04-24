using Microsoft.EntityFrameworkCore;
using VizsgaremekApp.Models;

namespace VizsgaremekApp.Models
{
    public class VizsgaremekContext : DbContext
    {
        public VizsgaremekContext(DbContextOptions<VizsgaremekContext> options)
            : base(options) { }

        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<Instructor> Instructors { get; set; }
        public DbSet<Package> Packages { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<InstructorStatusEntity> InstructorStatuses { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Instructor>(entity =>
                {
                    entity.Property(e => e.Name).HasColumnType("text");
                    entity.Property(e => e.Email).HasColumnType("text");
                    entity.Property(e => e.Phone).HasColumnType("text");
                    entity.Property(e => e.Address).HasColumnType("text");
                    entity.Property(e => e.Password).HasColumnType("text");
                    entity.Property(e => e.QualificationFileName).HasColumnType("text");
                    entity.Property(e => e.IdCardFileName).HasColumnType("text");
                    entity.Property(e => e.CVFileName).HasColumnType("text");
                    entity.Property(e => e.Status).HasColumnType("integer");
                    entity.Property(e => e.ApplicationDate).HasColumnType("timestamp");
                    entity.Property(e => e.HireDate).HasColumnType("timestamp");
                    entity.Property(e => e.IsActive).HasColumnType("boolean");
                });
            modelBuilder.Entity<Instructor>().HasData(
                new Instructor
                {
                    Id = 1,
                    Name = "Nagy Gergely",
                    Email = "gergely@loter.hu",
                    Phone = "+36301234567",
                    Address = "Budapest, Fő utca 1",
                    QualificationFileName = "/uploads/qualifications/default.pdf",
                    IdCardFileName = "/uploads/idcards/default.jpg",
                    CVFileName = "/uploads/cvs/default.pdf",
                    Status = InstructorStatus.Hired,  // Changed from Active to Hired
                    ApplicationDate = DateTime.UtcNow.AddMonths(-3),
                    HireDate = DateTime.UtcNow.AddMonths(-2),
                    IsActive = true,
                    Password = "test123"
                },
                new Instructor
                {
                    Id = 2,
                    Name = "Kovács Ákos",
                    Email = "akos@loter.hu",
                    Phone = "+36209876543",
                    Address = "Debrecen, Piac utca 5",
                    QualificationFileName = "/uploads/qualifications/default.pdf",
                    IdCardFileName = "/uploads/idcards/default.jpg",
                    CVFileName = "/uploads/cvs/default.docx",
                    Status = InstructorStatus.Hired,  // Changed from Active to Hired
                    ApplicationDate = DateTime.UtcNow.AddMonths(-1),
                    HireDate = DateTime.UtcNow.AddDays(-15),
                    IsActive = true,
                    Password = "test321"
                }
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

            modelBuilder.Entity<InstructorStatusEntity>().HasData(
                new InstructorStatusEntity { Id = 1, Type = "Pending" },
                new InstructorStatusEntity { Id = 2, Type = "Approved" },
                new InstructorStatusEntity { Id = 3, Type = "Hired" },
                new InstructorStatusEntity { Id = 4, Type = "Rejected" },
                new InstructorStatusEntity { Id = 5, Type = "OnLeave" },
                new InstructorStatusEntity { Id = 6, Type = "Terminated" }
            );
        }
    }
}
