using System;
using System.Collections.Generic;
using IGSLControlPanel.Models.ManyToManyLinks;

namespace IGSLControlPanel.Models
{
    public class Product : BaseModel
    {
        public Guid FolderId { get; set; }

        public List<ProductLinkToProductParameter> LinkToProductParameters { get; set; } = new List<ProductLinkToProductParameter>();
    }
}
