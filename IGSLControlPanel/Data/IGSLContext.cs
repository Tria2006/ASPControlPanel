using IGSLControlPanel.Models;
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
    }
}
