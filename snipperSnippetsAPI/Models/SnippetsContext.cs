using Microsoft.EntityFrameworkCore;

namespace SnipperSnippets.Models
{
    public class SnippetsContext : DbContext 
    {
        public SnippetsContext(DbContextOptions<SnippetsContext> options) : base(options)
        {
        }

        public DbSet<Snippet> Snippets { get; set; }
    }
}