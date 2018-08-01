using System.Collections.Generic;
using IGSLControlPanel.Models.ManyToManyLinks;

namespace IGSLControlPanel.Models
{
    public class InsuranceRule : BaseModel
    {
        public List<InsRuleTariffLink> LinksToTariff { get; set; } = new List<InsRuleTariffLink>();
    }
}
