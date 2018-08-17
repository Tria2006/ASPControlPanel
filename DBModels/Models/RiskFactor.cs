using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace DBModels.Models
{
    public class RiskFactor : BaseModel
    {
        [NotMapped]
        public List<Risk> Risks { get; set; }

        public List<FactorValue> FactorValues { get; set; }
    }
}
