using System;

namespace IGSLControlPanel.Models
{
    public class ParameterGroup
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public bool IsDeleted { get; set; }
    }
}
