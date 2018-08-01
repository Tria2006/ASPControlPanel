﻿using System;
using System.Collections.Generic;

namespace DBModels.Models
{
    public class Tariff : BaseModel
    {
        public Guid FolderId { get; set; }

        public List<InsRuleTariffLink> InsRuleTariffLink { get; set; } = new List<InsRuleTariffLink>();
    }
}
