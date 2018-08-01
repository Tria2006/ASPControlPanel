using System.Collections.Generic;
using DBModels.Models.ManyToManyLinks;

namespace DBModels.Models
{
    public class Risk : BaseModel
    {
        public int BaseTariffValue { get; set; }

        public int BaseTariffType { get; set; }

        public List<RiskInsRuleLink> LinksToInsRules { get; set; } = new List<RiskInsRuleLink>();

        public List<RiskRequirement> Requirements { get; set; } = new List<RiskRequirement>();
    }
}
