using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IGSLControlPanel.Models
{
    public class Tariff
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [DisplayName("Название")]
        public string Name { get; set; }

        [DataType(DataType.Date)]
        [DisplayName("Действителен с")]
        public DateTime? ValidFrom { get; set; }

        [DataType(DataType.Date)]
        [DisplayName("Действителен по")]
        public DateTime? ValidTo { get; set; }

        public bool IsDeleted { get; set; }

        public Guid FolderId { get; set; }
    }
}
