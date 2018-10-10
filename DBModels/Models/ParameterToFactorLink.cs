using System;

namespace DBModels.Models
{
    public class ParameterToFactorLink : BaseModel
    {
        public Guid ProductId { get; set; }

        public Guid TariffId { get; set; }

        public Guid ProductParameterId { get; set; }

        public Guid RiskFactorId { get; set; }
    }
}
