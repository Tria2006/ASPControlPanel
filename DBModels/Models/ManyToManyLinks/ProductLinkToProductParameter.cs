using System;

namespace DBModels.Models.ManyToManyLinks
{
    public class ProductLinkToProductParameter
    {
        public Guid ProductId { get; set; }

        public Product Product { get; set; }

        public Guid ProductParameterId { get; set; }

        public ProductParameter Parameter { get; set; }
    }
}
