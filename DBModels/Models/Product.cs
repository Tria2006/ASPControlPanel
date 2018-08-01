using System;
using System.Collections.Generic;
using DBModels.Models.ManyToManyLinks;

namespace DBModels.Models
{
    public class Product : BaseModel
    {
        public Guid FolderId { get; set; }

        public List<ProductLinkToProductParameter> LinkToProductParameters { get; set; } = new List<ProductLinkToProductParameter>();
    }
}
