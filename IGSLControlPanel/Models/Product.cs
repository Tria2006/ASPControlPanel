﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace IGSLControlPanel.Models
{
    public class Product
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        public string Name { get; set; }

        public Guid? FolderId { get; set; }

        public List<ProductParameter> ProductParameters { get; set; } = new List<ProductParameter>();

        public bool IsDeleted { get; set; }
    }
}
