using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using DBModels.Models.ManyToManyLinks;

namespace DBModels.Models
{
    public class Product : BaseModel
    {
        public Guid FolderId { get; set; }

        public Guid? TariffId { get; set; }

        [NotMapped]
        public Tariff Tariff { get; set; }

        public List<ProductLinkToProductParameter> LinkToProductParameters { get; set; } = new List<ProductLinkToProductParameter>();
    }
}
