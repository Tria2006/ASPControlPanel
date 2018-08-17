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

        public List<RiskFactor> SelectedFactors { get; set; }

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
    }
}
