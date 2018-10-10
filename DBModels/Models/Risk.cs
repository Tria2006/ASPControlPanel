using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using DBModels.Models.ManyToManyLinks;

namespace DBModels.Models
{
    public class Risk : BaseModel
    {
        public List<RiskInsRuleLink> LinksToInsRules { get; set; } = new List<RiskInsRuleLink>();

        public List<RiskRequirement> Requirements { get; set; } = new List<RiskRequirement>();

        [NotMapped]
        public bool OnCreateRequired { get; set; }
    }
}
