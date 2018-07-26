﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IGSLControlPanel.Models
{
    public class FolderTreeEntry
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        public Guid? ParentFolderId { get; set; }

        [NotMapped]
        public List<FolderTreeEntry> ChildFolders { get; set; } = new List<FolderTreeEntry>();

        [NotMapped]
        public List<Product> Products { get; set; } = new List<Product>();

        [NotMapped]
        public List<Tariff> Tariffs { get; set; } = new List<Tariff>();

        [DisplayName("Название")]
        public string Name { get; set; }

        public bool IsDeleted { get; set; }

        public int ModelTypeId { get; set; }
    }
}
