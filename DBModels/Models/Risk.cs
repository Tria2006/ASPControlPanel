using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using DBModels.Models.ManyToManyLinks;

namespace DBModels.Models
{
    public class Risk : BaseModel
    {
        [DisplayName("Значение базового тарифа")]
        public int BaseTariffValue { get; set; }

        [DisplayName("Тип базового тарифа")]
        public int BaseTariffType { get; set; }

        public List<RiskInsRuleLink> LinksToInsRules { get; set; } = new List<RiskInsRuleLink>();

        public List<RiskRequirement> Requirements { get; set; } = new List<RiskRequirement>();

        [NotMapped]
        public bool OnCreateRequired { get; set; }
    }
}
