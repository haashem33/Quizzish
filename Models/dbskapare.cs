using Microsoft.EntityFrameworkCore;

namespace quizzish.Models
{// denna modul är koppling till db
    public class dbskapare : DbContext
    {
        public DbSet<Konto> Kontons { get; set; }
        public DbSet<fragor> fragors { get; set; }
        public DbSet<svar> svars { get; set; }
        public dbskapare() => Database.EnsureCreated();

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseSqlite("DataSource = databasen.db"); //Databasfilens namn
        }
    }
}
