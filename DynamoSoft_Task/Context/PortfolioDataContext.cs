using DynamoSoft_Task.Models;
using Microsoft.EntityFrameworkCore;

namespace DynamoSoft_Task.Context
{
    public class PortfolioDataContext : DbContext
    {
        public DbSet<PortfolioEntry> PortfolioEntries { get; set; }
        public DbSet<SymbolData> SymbolData { get; set; }

        public PortfolioDataContext(DbContextOptions<PortfolioDataContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<PortfolioEntry>().ToTable("PortfolioEntry");
            modelBuilder.Entity<SymbolData>().ToTable("SymbolData");
        }
    }
}
