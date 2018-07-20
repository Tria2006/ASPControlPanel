﻿using System;

namespace IGSLControlPanel.Models
{
    public class ValueLimit
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public int ParameterDataType { get; set; }

        public int? IntValueFrom { get; set; }

        public int? IntValueTo { get; set; }

        public DateTime? DateValueFrom { get; set; }

        public DateTime? DateValueTo { get; set; }

        public bool IsDeleted { get; set; }

        public Guid ParameterId { get; set; }

        public Guid ProductId { get; set; }
    }
}
