using System;

namespace DBModels.Models
{
    public class Coefficient : BaseModel
    {
        public Guid RiskId { get; set; }

        public Guid FactorValueId { get; set; }

        public double Value { get; set; }
    }
}
