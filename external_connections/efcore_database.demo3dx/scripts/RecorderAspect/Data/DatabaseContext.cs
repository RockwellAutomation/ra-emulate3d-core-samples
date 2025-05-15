using Microsoft.EntityFrameworkCore;
using System.IO;

namespace Demo3D.Components {
    internal class DatabaseContext : DbContext {
        private string DatabasePath { get; }

        public DbSet<Record> Records { get; set; }

        public DatabaseContext(string databasePath) {
            DatabasePath = databasePath;
            Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
            optionsBuilder.UseSqlite($"Filename={DatabasePath}");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            modelBuilder.Entity<Record>()
                .HasKey(r => r.Id);
        }
    }
}
