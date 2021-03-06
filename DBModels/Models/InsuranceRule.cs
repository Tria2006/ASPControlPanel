﻿using System.Collections.Generic;
using DBModels.Models.ManyToManyLinks;

namespace DBModels.Models
{
    public class InsuranceRule : BaseModel
    {
        public List<InsRuleTariffLink> LinksToTariff { get; set; } = new List<InsRuleTariffLink>();

        public List<RiskInsRuleLink> LinksToRisks { get; set; } = new List<RiskInsRuleLink>();
    }
}
