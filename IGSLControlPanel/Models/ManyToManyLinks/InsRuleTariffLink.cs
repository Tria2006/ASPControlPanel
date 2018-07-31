using System;

namespace IGSLControlPanel.Models.ManyToManyLinks
{
    public class InsRuleTariffLink
    {
        public Guid TariffId { get; set; }

        public Tariff Tariff { get; set; }

        public Guid InsRuleId { get; set; }

        public InsuranceRule InsRule { get; set; }
    }
}
