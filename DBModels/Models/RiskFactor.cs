using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using DBModels.Models.ManyToManyLinks;

namespace DBModels.Models
{
    public class RiskFactor : BaseModel
    {
        [NotMapped]
        public List<InsRuleTariffLink> InsuranceRulesLinks { get; set; } = new List<InsRuleTariffLink>();

        public List<FactorValue> FactorValues { get; set; } = new List<FactorValue>();

        public List<RiskFactorTariffLink> RiskFactorsTariffLinks { get; set; } = new List<RiskFactorTariffLink>();
    }
}
