using IGSLControlPanel.Models;
using IGSLControlPanel.Models.ManyToManyLinks;
using Microsoft.EntityFrameworkCore;

namespace IGSLControlPanel.Data
{
    public class IGSLContext : DbContext
    {
        public IGSLContext (DbContextOptions<IGSLContext> options)
            : base(options)
        {
        }

        public DbSet<FolderTreeEntry> FolderTreeEntries { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Tariff> Tariffs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ProductLinkToProductParameter>().HasKey(p => new {p.ProductId, p.ProductParameterId});
        }
    }
}
