using DBModels.Models;
using DBModels.Models.ManyToManyLinks;
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
        public DbSet<Risk> Risks { get; set; }
        public DbSet<RiskRequirement> RiskRequirements { get; set; }
        public DbSet<FactorValue> FactorValues { get; set; }
        public DbSet<RiskFactor> RiskFactors { get; set; }
        public DbSet<ProductParameter> ProductParameters { get; set; }
        public DbSet<ValueLimit> ValueLimits { get; set; }
        public DbSet<ParameterGroup> ParameterGroups { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ProductLinkToProductParameter>().HasKey(p => new {p.ProductId, p.ProductParameterId});
            modelBuilder.Entity<InsRuleTariffLink>().HasKey(p => new {p.TariffId, p.InsRuleId});
            modelBuilder.Entity<RiskInsRuleLink>().HasKey(p => new {p.RiskId, p.InsRuleId});
        }
    }
}
