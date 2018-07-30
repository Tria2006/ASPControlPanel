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
        public DbSet<InsuranceRule> InsuranceRules { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ProductLinkToProductParameter>().HasKey(p => new {p.ProductId, p.ProductParameterId});
            modelBuilder.Entity<InsRuleTariffLink>().HasKey(p => new {p.TariffId, p.InsRuleId});
        }

        public DbSet<ProductParameter> ProductParameters { get; set; }

        public DbSet<ValueLimit> ValueLimits { get; set; }

        public DbSet<ParameterGroup> ParameterGroups { get; set; }

        public DbSet<IGSLControlPanel.Models.ManyToManyLinks.InsRuleTariffLink> InsRuleTariffLink { get; set; }
    }
}
