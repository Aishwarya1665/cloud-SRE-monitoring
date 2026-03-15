using Microsoft.EntityFrameworkCore;
using CloudSRE.API.Models;

namespace CloudSRE.API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Service> Services { get; set; }

        public DbSet<HealthLog> HealthLogs { get; set; }

        public DbSet<Incident> Incidents { get; set; }

        public DbSet<Alert> Alerts { get; set; }
    }
}