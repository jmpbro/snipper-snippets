using Microsoft.EntityFrameworkCore;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContextContext> options) : base (options) { }
    public DbSet<User> Users { get; set; }

}