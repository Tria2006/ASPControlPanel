using System;
using System.Collections.Generic;

namespace DBModels.Models
{
    public class FactorValue : BaseModel
    {
        public Guid TariffId { get; set; }

        public Guid RiskId { get; set; }

        public Guid RiskFactorId { get; set; }

        public List<Coefficient> Values { get; set; } = new List<Coefficient>();
    }
}
