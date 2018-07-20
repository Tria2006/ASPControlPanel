using System;
using System.Collections.Generic;
using IGSLControlPanel.Models.ManyToManyLinks;

namespace IGSLControlPanel.Models
{
    public class ProductParameter
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public int DataType { get; set; }

        public List<ProductLinkToProductParameter> LinkToProduct { get; set; }

        public bool IsDeleted { get; set; }

        public bool IsRequiredForCalc { get; set; }

        public bool IsRequiredForSave { get; set; }

        public int Order { get; set; }

        public ValueLimit Limit { get; set; }
    }
}
