﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using DBModels.Models.ManyToManyLinks;

namespace DBModels.Models
{
    public class Tariff : BaseModel
    {
        public Guid FolderId { get; set; }

        public List<InsRuleTariffLink> InsRuleTariffLink { get; set; } = new List<InsRuleTariffLink>();

        public List<RiskFactorTariffLink> RiskFactorsTariffLinks { get; set; } = new List<RiskFactorTariffLink>();

        public List<Product> LinkedProducts { get; set; } = new List<Product>();
    }
}
