using System;

namespace DBModels.Models
{
    public class FactorValue : BaseModel
    {
        public Guid TariffId { get; set; }

        public Guid RiskId { get; set; }

        public int Value { get; set; }
    }
}
