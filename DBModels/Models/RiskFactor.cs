using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using DBModels.Models.ManyToManyLinks;

namespace DBModels.Models
{
    public class RiskFactor : BaseModel
    {
        [NotMapped]
        public List<Risk> Risks { get; set; }

        public List<FactorValue> FactorValues { get; set; }

        public List<RiskFactorTariffLink> RiskFactorsTariffLinks { get; set; } = new List<RiskFactorTariffLink>();
    }
}
