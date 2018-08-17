using System;

namespace DBModels.Models.ManyToManyLinks
{
    public class RiskFactorTariffLink
    {
        public Guid RiskFactorId { get; set; }

        public RiskFactor RiskFactor { get; set; }

        public Guid TariffId { get; set; }

        public Tariff Tariff { get; set; }
    }
}
