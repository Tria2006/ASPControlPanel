using System;

namespace DBModels.Models.ManyToManyLinks
{
    public class RiskInsRuleLink
    {
        public Guid RiskId { get; set; }

        public Risk Risk { get; set; }

        public Guid InsRuleId { get; set; }

        public InsuranceRule InsRule { get; set; }
    }
}
