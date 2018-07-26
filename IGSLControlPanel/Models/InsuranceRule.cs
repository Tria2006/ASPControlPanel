using System;
using System.ComponentModel;

namespace IGSLControlPanel.Models
{
    public class InsuranceRule
    {
        public Guid Id { get; set; }

        [DisplayName("Название")]
        public string Name { get; set; }

        public bool IsDeleted { get; set; }
    }
}
