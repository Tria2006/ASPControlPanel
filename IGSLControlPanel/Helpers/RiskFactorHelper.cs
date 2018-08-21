using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DBModels.Models;
using IGSLControlPanel.Data;

namespace IGSLControlPanel.Helpers
{
    public class RiskFactorHelper
    {
        public RiskFactor CurrentFactor { get; set; }

        public List<RiskFactor> SelectedFactors { get; set; } = new List<RiskFactor>();

        public async Task CheckFactor(Guid id, IGSLContext context)
        {
            var factor = await context.RiskFactors.FindAsync(id);
            var factorChecked = SelectedFactors.Any(x => x.Id == id);
            if (factorChecked)
            {
                SelectedFactors.RemoveAll(x => x.Id == id);
            }
            else
            {
                SelectedFactors.Add(factor);
            }
        }

        public void PrepareFactorData(IGSLContext context)
        {
            foreach (var ruleLink in CurrentFactor.InsuranceRulesLinks)
            {
                foreach (var riskLink in ruleLink.InsRule.LinksToRisks)
                {
                    var factorValue = CurrentFactor.FactorValues
                        .SingleOrDefault(x => x.RiskId == riskLink.RiskId && x.RiskFactorId == CurrentFactor.Id && x.TariffId == ruleLink.TariffId);
                    if (factorValue == null)
                    {
                        factorValue = new FactorValue
                        {
                            RiskFactorId = CurrentFactor.Id,
                            RiskId = riskLink.RiskId,
                            TariffId = ruleLink.TariffId,
                            ValidFrom = DateTime.Now,
                            ValidTo = new DateTime(2100, 1, 1)
                        };
                        CurrentFactor.FactorValues.Add(factorValue);
                        context.FactorValues.Add(factorValue);
                        context.SaveChanges();
                    }
                }
            }
        }
    }
}
