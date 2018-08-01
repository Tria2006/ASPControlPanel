using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace IGSLControlPanel.Models
{
    public class FolderTreeEntry : BaseModel
    {
        public Guid? ParentFolderId { get; set; }

        [NotMapped]
        public List<FolderTreeEntry> ChildFolders { get; set; } = new List<FolderTreeEntry>();

        [NotMapped]
        public List<Product> Products { get; set; } = new List<Product>();

        [NotMapped]
        public List<Tariff> Tariffs { get; set; } = new List<Tariff>();

        public int ModelTypeId { get; set; }
    }
}
