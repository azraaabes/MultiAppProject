using Microsoft.EntityFrameworkCore;
using Webapiproje.Models;

namespace Webapiproje.DataDbContext
{
    public class ApplicationDbContext:DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext>options):base(options)
        {
            
        }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Log> Logs { get; set; }

    }
}
