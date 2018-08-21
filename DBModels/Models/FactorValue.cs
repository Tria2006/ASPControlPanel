using System;

namespace DBModels.Models
{
    public class FactorValue : BaseModel
    {
        public Guid TariffId { get; set; }

        public Guid RiskId { get; set; }

        public Guid RiskFactorId { get; set; }

        public double Value { get; set; }
    }
}
