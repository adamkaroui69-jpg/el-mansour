using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ElMansourSyndicManager.Infrastructure.Data
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            
            // Use SQLite for design-time migrations
            // The actual connection string doesn't matter for creating migrations, 
            // but the provider (UseSqlite) determines the SQL syntax generated.
            optionsBuilder.UseSqlite("Data Source=design_time.db");

            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}
