using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IGSLControlPanel.Models.ManyToManyLinks;

namespace IGSLControlPanel.Models
{
    public class Tariff
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [DisplayName("Название")]
        public string Name { get; set; }

        [DataType(DataType.Date)]
        [DisplayName("Действителен с")]
        public DateTime? ValidFrom { get; set; }

        [DataType(DataType.Date)]
        [DisplayName("Действителен по")]
        public DateTime? ValidTo { get; set; }

        public bool IsDeleted { get; set; }

        public Guid FolderId { get; set; }

        public List<InsRuleTariffLink> InsRuleTariffLink { get; set; } = new List<InsRuleTariffLink>();
    }
}
