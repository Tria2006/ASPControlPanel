using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DBModels.Models;
using IGSLControlPanel.Data;

namespace IGSLControlPanel.Helpers
{
    public class InsuranceRulesHelper
    {
        public List<InsuranceRule> SelectedRules { get; set; } = new List<InsuranceRule>();
        public InsuranceRule CurrentRule { get; set; }
        public Risk CurrentRisk { get; set; }

        public async Task CheckInsRule(Guid id, IGSLContext context)
        {
            var rule = await context.InsuranceRules.FindAsync(id);
            var ruleChecked = SelectedRules.Any(x => x.Id == id);
            if (ruleChecked)
            {
                SelectedRules.RemoveAll(x => x.Id == id);
            }
            else
            {
                SelectedRules.Add(rule);
            }
        }
    }
}
