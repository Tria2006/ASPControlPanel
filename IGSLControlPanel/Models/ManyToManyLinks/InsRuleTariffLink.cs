using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IGSLControlPanel.Models.ManyToManyLinks
{
    public class InsRuleTariffLink
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        public Guid TariffId { get; set; }

        public Tariff Tariff { get; set; }

        public Guid InsRuleId { get; set; }

        public InsuranceRule InsRule { get; set; }
    }
}
