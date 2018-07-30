using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IGSLControlPanel.Models.ManyToManyLinks;

namespace IGSLControlPanel.Models
{
    public class InsuranceRule
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [DisplayName("Название")]
        public string Name { get; set; }

        public List<InsRuleTariffLink> LinksToTariff { get; set; } = new List<InsRuleTariffLink>();

        public bool IsDeleted { get; set; }
    }
}
