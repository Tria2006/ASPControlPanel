using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IGSLControlPanel.Models.ManyToManyLinks;

namespace IGSLControlPanel.Models
{
    public class Product
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        public string Name { get; set; }

        [DataType(DataType.Date)]
        public DateTime ValidFrom { get; set; }

        [DataType(DataType.Date)]
        public DateTime ValidTo { get; set; }

        public Guid? FolderId { get; set; }

        public List<ProductLinkToProductParameter> LinkToProductParameters { get; set; }

        public bool IsDeleted { get; set; }
    }
}
