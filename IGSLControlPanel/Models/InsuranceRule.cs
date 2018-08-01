﻿using System.Collections.Generic;

namespace DBModels.Models
{
    public class InsuranceRule : BaseModel
    {
        public List<InsRuleTariffLink> LinksToTariff { get; set; } = new List<InsRuleTariffLink>();
    }
}
