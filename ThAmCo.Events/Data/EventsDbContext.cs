using Microsoft.EntityFrameworkCore;

namespace ThAmCo.Events.Data
{
    public class EventsDbContext : DbContext
    {
        public DbSet<Guest> Guests { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Staff> Staff { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<Qualifications> Qualifications { get; set; }

        public DbSet<StaffQualification> StaffQualifications { get; set; }


        public string DbPath { get; }

        public EventsDbContext()
        {
            var folder = Environment.SpecialFolder.ApplicationData;
            var path = Environment.GetFolderPath(folder);
            DbPath = Path.Join(path, "TheAmCo.Events.db");

        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.UseSqlite($"Data Source={DbPath}");
        }



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


            // Seed data for Staff
            modelBuilder.Entity<Staff>().HasData(
                new Staff { StaffId = 1, FirstName = "Bill", LastName = "Bog" },
                new Staff { StaffId = 2, FirstName = "Kevin", LastName = "Johnson" },
                new Staff { StaffId = 3, FirstName = "Mike", LastName = "Ross" }
            );

            // Seed data for Qualifications
            modelBuilder.Entity<Qualifications>().HasData(
                new Qualifications { QualificationId = 1, Name = "First Aider" },
                new Qualifications { QualificationId = 2, Name = "Bar Staff" },
                new Qualifications { QualificationId = 3, Name = "Waiter" },
                new Qualifications { QualificationId = 4, Name = "Security" },
                new Qualifications { QualificationId = 5, Name = "DJ" },
                new Qualifications { QualificationId = 6, Name = "Technician" }
            );



            // Seed data for StaffQualifications
            modelBuilder.Entity<StaffQualification>().HasData(
                new StaffQualification { StaffId = 1, QualificationId = 5 }, // DJ for Bill Bog
                new StaffQualification { StaffId = 2, QualificationId = 1 }, // First Aider for Kevin Johnson
                new StaffQualification { StaffId = 2, QualificationId = 4 }, // Security for Kevin Johnson
                new StaffQualification { StaffId = 3, QualificationId = 2 }  // Bar Staff for Mike Ross
            );


            // Composite key
            modelBuilder.Entity<StaffQualification>()
                .HasKey(sq => new { sq.StaffId, sq.QualificationId });

            // Relationship between Staff and StaffQualification
            modelBuilder.Entity<StaffQualification>()
                .HasOne(sq => sq.Staff)
                .WithMany(s => s.StaffQualifications)
                .HasForeignKey(sq => sq.StaffId);

            // Relationship between Qualification and StaffQualification
            modelBuilder.Entity<StaffQualification>()
                .HasOne(sq => sq.Qualification)
                .WithMany(q => q.StaffQualifications)
                .HasForeignKey(sq => sq.QualificationId);


        }
    }
}