using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DBModels.Models
{
    public class RiskRequirement
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        public Guid TariffId { get; set; }

        public Guid RiskId  { get; set; }

        public bool IsRequired { get; set; }
    }
}
