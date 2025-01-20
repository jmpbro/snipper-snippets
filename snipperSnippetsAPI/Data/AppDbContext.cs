using Microsoft.EntityFrameworkCore;
using SnipperSnippet.Models;

namespace SnipperSnippet.Data
{
    public class AppDbContext : IdentityDbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
    }
}
